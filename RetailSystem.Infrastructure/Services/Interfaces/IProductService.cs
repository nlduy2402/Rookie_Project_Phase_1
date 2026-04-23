using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface IProductService : IBaseService<Product>
    {
        Task<ServiceResult<List<Product>>> GetAllProductAsync();
        Task<ServiceResult<Product>> GetProductByIdAsync(int id);
        Task<ServiceResult<Product>> CreateAsync(CreateProductDTO model);
        Task<ServiceResult<Product>> UpdateAsync(UpdateProductDTO model);
        Task<ServiceResult<List<Product>>> GetByCategory(int id);
        new Task<ServiceResult<bool>> DeleteAsync(int id);

    }
}
