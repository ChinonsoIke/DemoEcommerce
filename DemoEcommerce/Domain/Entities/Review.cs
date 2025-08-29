namespace DemoEcommerce.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
    }
}
