using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<(IEnumerable<Product>, int totalCount)> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.Products.Include(p=>p.Images).AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<TopProductDTO>> GetTopSellingProductsAsync(int topCount, int status)
        {
            // Gọi Stored Procedure thông qua DbContext
            return await _context.Database
                .SqlQueryRaw<TopProductDTO>("EXEC USP_GetTopSellingProducts @TopCount={0}, @OrderStatus={1}", topCount, status)
                .ToListAsync();
        }
    }
}
