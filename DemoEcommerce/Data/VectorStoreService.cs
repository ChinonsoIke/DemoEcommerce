using DemoEcommerce.Application.Interfaces;
using DemoEcommerce.Domain.Entities;
using Microsoft.Extensions.VectorData;
using System.Linq.Expressions;

namespace DemoEcommerce.Data
{
    public class VectorStoreService<T> : IVectorStoreService<T> where T : BaseEntity
    {
        public VectorStoreCollection<Guid, T> _vectorCollection { get; set; }

        // search
        // upsert
        public VectorStoreService(VectorStoreCollection<Guid, T> vectorCollection)
        {
            _vectorCollection = vectorCollection;
        }

        public async Task Upsert(T item)
        {
            if (item == null) return;

            await _vectorCollection.UpsertAsync(item);
        }

        public async Task<List<T>> Search(ReadOnlyMemory<float> vector, Expression<Func<T, bool>> filter = null)
        {
            IAsyncEnumerable<VectorSearchResult<T>> result = _vectorCollection.SearchAsync(vector, 10, new VectorSearchOptions<T> { Filter = filter});
            var list = new List<T>();

            await foreach (VectorSearchResult<T> item in result)
            {
                list.Add(item.Record);
            }

            return list;
        }
    }
}
