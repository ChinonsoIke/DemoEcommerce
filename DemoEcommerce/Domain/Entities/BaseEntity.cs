using Microsoft.Extensions.VectorData;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoEcommerce.Domain.Entities
{
    public class BaseEntity
    {
        public bool Embedded { get; set; }
        [VectorStoreVector(384, DistanceFunction = DistanceFunction.CosineSimilarity)]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
