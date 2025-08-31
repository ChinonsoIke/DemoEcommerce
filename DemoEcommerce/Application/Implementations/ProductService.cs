using AutoMapper;
using Azure;
using DemoEcommerce.Application.DTOs;
using DemoEcommerce.Application.Interfaces;
using DemoEcommerce.Data;
using DemoEcommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DemoEcommerce.Application.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IAIService _aIService;
        private readonly InMemoryVectorStores _inMemoryVectorStores;
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private readonly ChatOptions _chatOptions;

        public ProductService(IAIService aIService, InMemoryVectorStores inMemoryVectorStores, AppDbContext dbContext,IMemoryCache memoryCache, IMapper mapper)
        {
            _aIService = aIService;
            _inMemoryVectorStores = inMemoryVectorStores;
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _mapper = mapper;
            _chatOptions = new ChatOptions
            {
                Tools = [AIFunctionFactory.Create(Search)]
            };
        }

        public List<ProductResponse> GetProducts()
        {
            var products = _dbContext.Products;
            var response = _mapper.Map<List<ProductResponse>>(products);

            return response;
        }

        public async Task<ProductResponse> GetProduct(Guid id)
        {
            Product product = _dbContext.Products.Include(p => p.Reviews).FirstOrDefault(p => p.Id == id);
            if (product == null) return null;

            var response = _mapper.Map<ProductResponse>(product);
            var reviewSummary = _memoryCache.Get($"reviews_{response.Id}");
            if(reviewSummary == null)
            {
                reviewSummary = await GetProductReviewsSummary(response);
            }
            response.ReviewSummary = (string) reviewSummary;

            return response;
        }

        public async IAsyncEnumerable<string> SingleProductRAGQuery(Guid productId, string query)
        {
            Product product = _dbContext.Products.Include(p => p.Reviews).FirstOrDefault(p => p.Id == productId);
            if (product == null) yield break;

            List<ChatMessage> messages = null;
            var chatHistory = _memoryCache.Get("rag_" + productId.ToString());
            if(chatHistory != null)
            {
                messages = (List<ChatMessage>)chatHistory;
                messages.Add(new ChatMessage(ChatRole.User, query));
            }
            else
            {
                var mapped = _mapper.Map<ProductResponse>(product);

                var queryEmbedding = await _aIService.GetEmbedding(query);
            
                var generalResults = await _inMemoryVectorStores.Service<Product>().Search(queryEmbedding);
                var generalMapped = _mapper.Map<List<ProductResponse>>(generalResults);

                var reviewSummary = _memoryCache.Get($"reviews_{mapped.Id}");
                if (reviewSummary == null)
                {
                    reviewSummary = await GetProductReviewsSummary(mapped);
                }
                mapped.ReviewSummary = (string)reviewSummary;

                string systemPrompt = $"""
                    You are a chat assistant for an e-commerce app.
                    This is the product the user is inquiring about:
                    {JsonConvert.SerializeObject(mapped)}

                    If the question seems specific, then answer relating only to the product.

                    If the question is more generalized, then you can make use of the Search tool to get more context.

                    Answer like an actual assistant trying to provide meaningful information.

                    Answer only questions related to the context of our e-commerce app.
                    If the question is not related prompt the user to
                    ask another question.
                    """;
                messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, query)
                };
            }


            var chat = _aIService.GetChatCompletion(messages, _chatOptions);
            StringBuilder sb = new ();

            await foreach(string chunk in chat)
            {
                sb.Append(chunk);
                yield return chunk;
            }

            messages.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));
            _memoryCache.Set("rag_" + productId.ToString(), messages);
        }

        public async Task EmbedPendingItems()
        {
            var products = _dbContext.Products.Where(p => !p.Embedded);
            await _inMemoryVectorStores._productVectorCollection.EnsureCollectionExistsAsync();

            foreach (var product in products)
            {
                var embedding = await _aIService.GetEmbedding(JsonConvert.SerializeObject(product));
                product.Embedding = embedding;
                await _inMemoryVectorStores._productVectorCollection.UpsertAsync(product);
                product.Embedded = true;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task PopulateVectorStores()
        {
            var products = _dbContext.Products.Where(p => p.Embedded);
            await _inMemoryVectorStores._productVectorCollection.EnsureCollectionExistsAsync();

            foreach (var product in products)
            {
                await _inMemoryVectorStores._productVectorCollection.UpsertAsync(product);
            }
        }

        public async Task<string> GetProductReviewsSummary(ProductResponse productResponse)
        {
            if (!productResponse.Reviews.Any()) return "";

            string prompt = $"""
                You are a chat assistant for an ecommerce app.
                Provide a summary of the reviews for the provided product, to enable the customer have an idea of the general sentiments of previous buyers.
                {JsonConvert.SerializeObject(productResponse)}
                """
            ;

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, prompt)
            };
            var summary = new StringBuilder();

            var response = _aIService.GetChatCompletion(messages, _chatOptions);
            await foreach (var chunk in response)
            {
                summary.Append(chunk);
            }

            _memoryCache.Set($"reviews_{productResponse.Id}", summary.ToString());
            return summary.ToString();
        }

        [Description("Searches the vector to get more context")]
        public async Task<List<Product>> Search(
            [Description("This is the last message sent by the user.")] string userMessage, 
            //[Description("The names of OTHER products to filter by. If not specified we may search in all products. If it does not apply then leave null.")] string? productName,
            [Description("The category id to filter by. Use this to search for similar products. If not specified we may search in all categories.")] string? categoryId)
        {
            try
            {
                Console.WriteLine("searching...");
                var queryEmbedding = await _aIService.GetEmbedding(userMessage);
                //if(productName != null)
                //{
                //    var products = _dbContext.Products.Where(p => p.Name.Contains(productName, StringComparison.OrdinalIgnoreCase));
                //    if (!products.Any()) productName = null;
                //}
                if(!string.IsNullOrEmpty(categoryId))
                {
                    var category = _dbContext.Categories.Find(Guid.Parse(categoryId));
                    if (category == null) categoryId = null;
                }

                var response = await _inMemoryVectorStores.Service<Product>().Search(queryEmbedding, filter: p =>
                    //((productName != null && p.Name.Contains(productName, StringComparison.OrdinalIgnoreCase)) || productName == null)
                    //&&
                    ((categoryId != null && p.CategoryId == Guid.Parse(categoryId)) || categoryId == null)
                );

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
