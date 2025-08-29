namespace DemoEcommerce.Application.DTOs
{
    public class ReviewResponse
    {
        Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
    }
}
