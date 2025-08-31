using Microsoft.Extensions.AI;

namespace DemoEcommerce.Application.Interfaces
{
    public interface IAIService
    {
        Task<ReadOnlyMemory<float>> GetEmbedding(string data);
        IAsyncEnumerable<string> GetChatCompletion(string query, string data);
        IAsyncEnumerable<string> GetChatCompletion(List<ChatMessage> chatMessages, ChatOptions options);
    }
}
