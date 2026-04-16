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
        Task<ProductDTO> CreateAsync(CreateProductDTO model);
    }
}
