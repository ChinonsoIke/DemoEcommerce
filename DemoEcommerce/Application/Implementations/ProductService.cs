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
using System.Text;

namespace DemoEcommerce.Application.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IAIService _aIService;
        private readonly InMemoryVectorStores _inMemoryVectorStores;
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;

        public ProductService(IAIService aIService, InMemoryVectorStores inMemoryVectorStores, AppDbContext dbContext,IMemoryCache memoryCache, IMapper mapper)
        {
            _aIService = aIService;
            _inMemoryVectorStores = inMemoryVectorStores;
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _mapper = mapper;
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
            var queryEmbedding = await _aIService.GetEmbedding(query);
            var results = await _inMemoryVectorStores.Service<Product>().Search(queryEmbedding, filter: p => p.Id == productId);
            var mapped = _mapper.Map<List<ProductResponse>>(results);

            foreach ( var result in mapped)
            {
                var reviewSummary = _memoryCache.Get($"reviews_{result.Id}");
                if (reviewSummary == null)
                {
                    reviewSummary = await GetProductReviewsSummary(result);
                }
                result.ReviewSummary = (string)reviewSummary;
            }

            var chat = _aIService.GetChatCompletion(query, JsonConvert.SerializeObject(mapped));
            await foreach(string chunk in chat)
            {
                yield return chunk;
            }
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

            var response = _aIService.GetChatCompletion(messages);
            await foreach (var chunk in response)
            {
                summary.Append(chunk);
            }

            _memoryCache.Set($"reviews_{productResponse.Id}", summary.ToString());
            return summary.ToString();
        }
    }
}
