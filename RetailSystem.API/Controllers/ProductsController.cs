using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace RetailSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : BaseController
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllProductAsync();
            if (!result.IsSuccess) return BadRequest(result.Message);
            return OkResponse(result.Data);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {

            return OkResponse(await _productService.GetByIdAsync(id));
        }

        // POST api/products
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateProductDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _productService.CreateAsync(model);
            if(!result.IsSuccess) return BadRequest(result.Message);
            return OkResponse(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id,UpdateProductDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _productService.UpdateAsync(model);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return OkResponse(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.Message); // 404
            }

            return OkResponse(id);

        }
    }
}
