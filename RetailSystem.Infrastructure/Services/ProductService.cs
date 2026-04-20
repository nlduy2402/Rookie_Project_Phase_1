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
using RetailSystem.Shared.Extensions;

namespace RetailSystem.Infrastructure.Services
{
    public class ProductService : BaseService<Product>, IProductService
    {
        public ProductService(AppDbContext context) : base(context)
        {
        }

        // override nếu cần include
        public async Task<ServiceResult<List<Product>>> GetAllProductAsync()
        {
            var result = await _context.Products
                            .Include(p => p.Images)
                            .Include(p => p.Category)
                            .ToListAsync();
            if (result == null) {
                return new ServiceResult<List<Product>>()
                {
                    IsSuccess = false,
                    Message = "Wrong"
                };
            }
            return new ServiceResult<List<Product>>()
            {
                IsSuccess = true,
                Data = result
            };
        }

        public async Task<ServiceResult<Product>> GetProductByIdAsync(int id)
        {
            var product =  await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) {
                return new ServiceResult<Product> { IsSuccess = false, Message = "Product not exist !" };
            }
            return new ServiceResult<Product>()
            {
                IsSuccess = true,
                Data = product
            };
        }

        public async Task<ServiceResult<Product>> UpdateAsync(UpdateProductDTO model)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (product == null)
            {
                return new ServiceResult<Product>() { IsSuccess = false, Message = "Data not found!" };

            }
            product?.UpdateFromDto(model);
            await _context.SaveChangesAsync();
            return new ServiceResult<Product>()
            {
                IsSuccess = true,
                Data = product
            };
        }


        public async Task<ServiceResult<Product>> CreateAsync(CreateProductDTO model)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == model.CategoryId);

            if (category == null)
                return new ServiceResult<Product>() { IsSuccess = false, Message = "Data not found!" };
            
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                RAM = model.RAM,
                SSD = model.SSD,
                ChipSet = model.ChipSet,
                CategoryId = model.CategoryId,

                Images = model.ImageUrls.Select(url => new ProductImage
                {
                    Url = url
                }).ToList()
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return new ServiceResult<Product>() { IsSuccess = false, Message = "Product Created!", Data=product };
        }
    }
}
