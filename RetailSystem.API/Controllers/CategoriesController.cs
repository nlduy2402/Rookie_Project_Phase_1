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
            var result = await _categoryService.GetAllAsync();
            if (!result.IsSuccess) return BadRequest(result.Message);
            return OkResponse(result.Data);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) {
            if(id <= 0) return BadRequest("Invalid input !");

            var result = await _categoryService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(result.Message);

            return OkResponse(result.Data,result.Message);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDTO model)
        {
            if (model == null) return BadRequest("Invalid input !");
            var result = await _categoryService.CreateAsync(model);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return OkResponse(result.Data, result.Message);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDTO model)
        {
            if(id <= 0 || model==null) return BadRequest("Invalid input !");

            var result = await _categoryService.UpdateAsync(id,model);
            if (!result.IsSuccess) return NotFound(result.Message);
            return OkResponse(result.Data,result.Message);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid input !");
            
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
