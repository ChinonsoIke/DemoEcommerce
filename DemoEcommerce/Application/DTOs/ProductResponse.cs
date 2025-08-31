namespace DemoEcommerce.Application.DTOs
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReviewSummary { get; set; }
        public Guid CategoryId { get; set; }

        public List<ReviewResponse> Reviews { get; set; } = [];
    }
}
