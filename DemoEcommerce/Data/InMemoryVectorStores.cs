using DemoEcommerce.Application.Interfaces;
using DemoEcommerce.Domain.Entities;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace DemoEcommerce.Data
{
    public class InMemoryVectorStores
    {
        public VectorStoreCollection<Guid, Product> _productVectorCollection { get; set; }

        public Dictionary<Type, object> _vectorStoreServices { get; set; }

        public InMemoryVectorStores()
        {
            var vectorStore = new InMemoryVectorStore();
            _productVectorCollection = vectorStore.GetCollection<Guid, Product>("ProductVectorStore");

            _vectorStoreServices = new Dictionary<Type, object>()
            {
                {typeof(Product), new VectorStoreService<Product>(_productVectorCollection)}
            };
        }

        public IVectorStoreService<T> Service<T>() where T : BaseEntity
        {
            return (IVectorStoreService<T>) _vectorStoreServices[typeof(T)];
        }
    }
}
