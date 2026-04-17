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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService) {
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _categoryService.GetAllAsync());
        }
        [HttpGet("id")]
        public async Task<IActionResult> GetById(int id) {
            return Ok(await _categoryService.GetByIdAsync(id));
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDTO model)
        {
            try
            {
                var result = await _categoryService.CreateAsync(model);
                return Ok(result);
            }
            catch (Exception ex) {
            }
            throw new Exception("Bad Request");
        }

        [HttpPut("id")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDTO model)
        {
            try
            {
                var result = await _categoryService.UpdateAsync(id,model);
                if (!result.IsSuccess) return NotFound(result.Message);
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
            }
            throw new Exception("Bad Request");
        }

        [HttpDelete("delete")]
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
