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
    public class ProductService : BaseService<Product>, IProductService
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

        public async Task<ProductDTO> CreateAsync(CreateProductDTO model)
        {
            var categories = await _context.Categories
                .Where(c => model.CategoryIds.Contains(c.Id))
                .ToListAsync();

            Product p = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Quantity = model.Quantity,
                Price = model.Price,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now,
                ImageUrl = model.ImageUrl,
                Categories = categories
            };
            _context.Products.Add(p);
            await _context.SaveChangesAsync();
            var result = new ProductDTO
            {
                Name = p.Name,
                Price = p.Price,
                Status = p.Status.ToString(),
                Categories = p.Categories.Select(c => c.Name).ToList()
            };

            return result;
        }
    }
}
