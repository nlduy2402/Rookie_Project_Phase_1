using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services
{
    public class ProductService : BaseService<Product>
    {
        public ProductService(AppDbContext context) : base(context)
        {
        }

        // override nếu cần include
        public override async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Categories)
                .ToListAsync();
        }
    }
}
