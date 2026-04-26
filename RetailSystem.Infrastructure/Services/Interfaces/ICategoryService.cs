using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface ICategoryService : IBaseService<Category>
    {
        new Task<ServiceResult<List<Category>>> GetAllAsync();
        Task<ServiceResult<Category>> CreateAsync(CreateCategoryDTO model);
        Task<ServiceResult<Category>> UpdateAsync(int id, UpdateCategoryDTO model);
        new Task<ServiceResult<Category>> GetByIdAsync(int id);
    }
}
