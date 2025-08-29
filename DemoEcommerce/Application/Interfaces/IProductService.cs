using DemoEcommerce.Application.DTOs;

namespace DemoEcommerce.Application.Interfaces
{
    public interface IProductService
    {
        IAsyncEnumerable<string> SingleProductRAGQuery(Guid productId, string query);
        Task EmbedPendingItems();
        Task PopulateVectorStores();
        List<ProductResponse> GetProducts();
        Task<ProductResponse> GetProduct(Guid id);
    }
}
