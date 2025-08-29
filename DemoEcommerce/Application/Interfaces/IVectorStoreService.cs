using DemoEcommerce.Domain.Entities;
using System.Linq.Expressions;

namespace DemoEcommerce.Application.Interfaces
{
    public interface IVectorStoreService<T> where T : BaseEntity
    {
        Task Upsert(T item);
        Task<List<T>> Search(ReadOnlyMemory<float> vector, Expression<Func<T, bool>> filter = null);
    }
}
