using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;

namespace RetailSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try {
                var result = await _categoryService.GetAllAsync();
                return OkResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all categories.");
                return StatusCode(500, "Internal server error");
            }
            throw new Exception("Bad Request");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) {
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(result.Message);

            return OkResponse(result.Data,result.Message);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDTO model)
        {
            try
            {
                var result = await _categoryService.CreateAsync(model);
                return OkResponse(result.Data, result.Message);
            }
            catch (Exception ex)
            {
            }
            throw new Exception("Bad Request");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDTO model)
        {
            try
            {
                var result = await _categoryService.UpdateAsync(id,model);
                if (!result.IsSuccess) return NotFound(result.Message);
                return OkResponse(result.Data,result.Message);
            }
            catch (Exception ex)
            {
            }
            throw new Exception("Bad Request");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid input !");
            }
            try
            {
                var result = await _categoryService.DeleteAsync(id);
                return Ok(result);
            }
            catch (Exception ex) {
                throw new Exception("Can not do this !");
            }
        }

    }
}
