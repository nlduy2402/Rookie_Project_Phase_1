using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Base;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace RetailSystem.Infrastructure.Services
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly IMemoryCache _cache;
        public CategoryService(AppDbContext context,ILogger<CategoryService> logger, IMemoryCache cache) : base(context, cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async new Task<ServiceResult<List<Category>>> GetAllAsync()
        {
            string _cacheKey = "AllCategories";
            if (!_cache.TryGetValue(_cacheKey, out List<Category>? categories))
            {

                categories = await _context.Categories.ToListAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) 
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                // 4. Lưu vào cache
                _cache.Set(_cacheKey, categories, cacheEntryOptions);
            }
            return new ServiceResult<List<Category>>
            {
                IsSuccess = true,
                Data = categories
            };
        }

        public async Task<ServiceResult<Category>> CreateAsync(CreateCategoryDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException("Name is required");
            }
            Category category = new Category
            {
                Name = model.Name,
                Description = model.Description,
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _cache.Remove("AllCategories");
            return new ServiceResult<Category> { Data = category, IsSuccess = true, Message="Category Created !"};
        }
        public async new Task<ServiceResult<Category>> GetByIdAsync(int id)
        {
            string cacheKey = $"Category_{id}";
            if (!_cache.TryGetValue(cacheKey, out Category? category))
            {
                category = await _dbSet.FindAsync(id);
                if (category == null)
                {
                    return new ServiceResult<Category> { IsSuccess = false, Message = "Dont't Have Any Category Match Input" };
                }
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set(cacheKey, category, cacheEntryOptions);
            }
            return new ServiceResult<Category> { IsSuccess = true, Data = category };
        }
        public async Task<ServiceResult<Category>> UpdateAsync(int id, UpdateCategoryDTO model)
        {
            var result = await GetByIdAsync(id);

            if (!result.IsSuccess || result.Data == null)
            {
                return new ServiceResult<Category>
                {
                    IsSuccess = false,
                    Message = "Error Occured While Updating Category"
                };
            }

            var category = result.Data;

            category.Name = model.Name;
            category.Description = model.Description;

            await _context.SaveChangesAsync();

            _cache.Remove("AllCategories");
            _cache.Remove($"Category_{id}");

            return new ServiceResult<Category>
            {
                IsSuccess = true,
                Data = category,
                Message = "Category Updated !"
            };
        }
    }
}
