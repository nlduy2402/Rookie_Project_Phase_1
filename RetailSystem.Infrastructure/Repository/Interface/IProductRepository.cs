using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository.Interface
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<(IEnumerable<Product>, int totalCount)> GetPagedAsync(int page, int pageSize);
        Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int topCount, int status);
    }
}
