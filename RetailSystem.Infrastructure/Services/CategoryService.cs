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
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Shared.ResponseModels;

namespace RetailSystem.Infrastructure.Services
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly new IMemoryCache _cache;
        private readonly new IUnitOfWork _uow;
        public CategoryService(ILogger<CategoryService> logger, IMemoryCache cache, IUnitOfWork uow) : base(uow, cache)
        {
            _logger = logger;
            _cache = cache;
            _uow = uow;
        }
        protected override IBaseRepository<Category> GetRepository() => _uow.Categories;

        public async new Task<ServiceResult<List<Category>>> GetAllAsync()
        {
            string _cacheKey = "AllCategories";
            if (!_cache.TryGetValue(_cacheKey, out List<Category>? categories))
            {

                var result = await _uow.Categories.GetAllAsync();
                if (result == null)
                {
                    return new ServiceResult<List<Category>>
                    {
                        IsSuccess = true,
                        Data = result?.ToList()
                    };
                }
                categories = result.ToList();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) 
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

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
            await _uow.Categories.CreateAsync(category);
            await _uow.SaveChangesAsync();

            _cache.Remove("AllCategories");
            return new ServiceResult<Category> { Data = category, IsSuccess = true, Message="Category Created !"};
        }
        public async new Task<ServiceResult<Category>> GetByIdAsync(int id)
        {
            string cacheKey = $"Category_{id}";
            if (!_cache.TryGetValue(cacheKey, out Category? category))
            {
                category = await _uow.Categories.GetByIdAsync(id);
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

            _uow.Categories.Update(category);
            await _uow.SaveChangesAsync();

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
