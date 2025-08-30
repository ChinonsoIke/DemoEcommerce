using DemoEcommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DemoEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public ActionResult GetProducts()
        {
            var response = _productService.GetProducts();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(Guid id)
        {
            var response = await _productService.GetProduct(id);
            return Ok(response);
        }

        [HttpGet("query/{productId}")]
        public async Task SingleProductRAGQuery(Guid productId, string query)
        {
            Response.ContentType = "text/plain"; // or "text/event-stream" for SSE

            await foreach (var chunk in _productService.SingleProductRAGQuery(productId, query))
            {
                var bytes = Encoding.UTF8.GetBytes(chunk);
                await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                await Response.Body.FlushAsync(); // ensure client receives it immediately
            }
        }
    }
}
