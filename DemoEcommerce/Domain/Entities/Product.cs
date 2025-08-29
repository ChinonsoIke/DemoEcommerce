using Microsoft.Extensions.VectorData;

namespace DemoEcommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        [VectorStoreKey]
        public Guid Id { get; set; }
        [VectorStoreData]
        public string Name { get; set; }
        [VectorStoreData]
        public string Description { get; set; }
        [VectorStoreData]
        public Guid CategoryId { get; set; }

        public ICollection<Review> Reviews { get; set; } = [];
    }
}
