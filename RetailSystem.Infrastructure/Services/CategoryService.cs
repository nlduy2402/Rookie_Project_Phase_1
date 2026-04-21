using Microsoft.EntityFrameworkCore;
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

namespace RetailSystem.Infrastructure.Services
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        public CategoryService(AppDbContext context) : base(context)
        {

        }

        public async Task<Category> CreateAsync(CreateCategoryDTO model)
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
            return category;
        }
        public async new Task<ServiceResult<Category>> GetByIdAsync(int id)
        {
            var category = await _dbSet.FindAsync(id);
            if (category == null)
            {
                return new ServiceResult<Category> { IsSuccess = false, Message = "Data not found" };
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
                    Message = "Data not found"
                };
            }

            var category = result.Data;

            category.Name = model.Name;
            category.Description = model.Description;

            await _context.SaveChangesAsync();

            return new ServiceResult<Category>
            {
                IsSuccess = true,
                Data = category
            };
        }







    }
}
