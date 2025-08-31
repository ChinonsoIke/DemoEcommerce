using DemoEcommerce.Application.Interfaces;
using Microsoft.Extensions.AI;
using OpenAI;

namespace DemoEcommerce.Application.Implementations
{
    public class AIService : IAIService
    {
        // getembedding
        // getchatcompletion
        public IEmbeddingGenerator<string, Embedding<float>> _embeddingClient { get; set; }
        public IChatClient _chatClient { get; set; }

        public AIService(IConfiguration config)
        {
            var client = new OpenAIClient(config["OpenAI:ApiKey"]);
            _embeddingClient = client
                .GetEmbeddingClient(config["OpenAI:EmbeddingModel"])
                .AsIEmbeddingGenerator();
            _chatClient =
                new ChatClientBuilder(client.GetChatClient(config["OpenAI:ChatModel"]).AsIChatClient())
                .UseFunctionInvocation()
                .Build();
        }

        public async Task<ReadOnlyMemory<float>> GetEmbedding(string data)
        {
            ReadOnlyMemory<float> embedding = await _embeddingClient.GenerateVectorAsync(data);
            return embedding;
        }

        public async IAsyncEnumerable<string> GetChatCompletion(string query, string data)
        {
            string systemPrompt = """
                You are a chat assistant for an e-commerce app.
                Answer only questions related to this context.
                If the question is not related to the context prompt the user to
                ask another question.
                If you are unsure, do not answer, just respond that you are unsure.

                """;
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt + data),
                new ChatMessage(ChatRole.User, query)
            };

            var response = _chatClient.GetStreamingResponseAsync(messages);

            await foreach (var chunk in response)
            {
                yield return chunk.Text;
            }
        }

        public async IAsyncEnumerable<string> GetChatCompletion(List<ChatMessage> chatMessages, ChatOptions options)
        {
            var response = _chatClient.GetStreamingResponseAsync(chatMessages, options);

            await foreach (var chunk in response)
            {
                yield return chunk.Text;
            }
        }
    }
}
