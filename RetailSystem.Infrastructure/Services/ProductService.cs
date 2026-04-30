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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using RetailSystem.Infrastructure.Repository.Interface;
using Microsoft.Identity.Client;
using RetailSystem.Domain.Repository.Interface;

namespace RetailSystem.Infrastructure.Services
{
    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly new IUnitOfWork _uow;
        private const string ProductCacheKey = "AllProducts";
        private readonly MemoryCacheEntryOptions _cacheOptions;
        public ProductService(ILogger<ProductService> logger, IMemoryCache cache,IUnitOfWork uow) : base(uow, cache)
        {
            _logger = logger;
            _uow = uow;
            _cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
        }
        protected override IBaseRepository<Product> GetRepository() => _uow.Products;

        public async Task<ServiceResult<List<Product>>> GetAllProductAsync()
        {
            if (!_cache.TryGetValue(ProductCacheKey, out List<Product>? products))
            {
                var resultFromDb = await _uow.Products.GetAllAsync(includeProperties: "Images,Category");
                products = resultFromDb.ToList();

                _cache.Set(ProductCacheKey, products, _cacheOptions);
            }

            return new ServiceResult<List<Product>> { IsSuccess = true, Data = products };
        }

        public async Task<ServiceResult<Product>> GetProductByIdAsync(int id)
        {
            var product = await _uow.Products.GetFirstOrDefaultAsync(p => p.Id == id, "Images,Category");

            if (product == null) return new ServiceResult<Product> { IsSuccess = false, Message = "Product Not Exist" };

            return new ServiceResult<Product> { IsSuccess = true, Data = product };
        }
        public async Task<ServiceResult<List<Product>>> GetByCategory(int id)
        {
            string _cacheKey = $"Products_Category_{id}";

            if (!_cache.TryGetValue(_cacheKey, out List<Product>? products))
            {
                _logger.LogInformation($"Cache miss for category {id}. Fetching from database...");

                // call Repo by UoW with filter and include
                var resultFromDb = await _uow.Products.GetAllAsync(
                    filter: p => p.CategoryId == id,
                    includeProperties: "Images,Category"
                );

                products = resultFromDb.ToList();

                if (products == null) products = new List<Product>();

                _cache.Set(_cacheKey, products, _cacheOptions);
            }

            if (!products.Any())
            {
                return new ServiceResult<List<Product>>
                {
                    IsSuccess = true,
                    Message = "No Product Found in This Category!",
                    Data = products
                };
            }

            return new ServiceResult<List<Product>> { IsSuccess = true, Data = products };
        }

        public async Task<ServiceResult<Product>> CreateAsync(CreateProductDTO model)
        {
            var category = await _uow.Categories.GetByIdAsync(model.CategoryId);
            if (category == null) return new ServiceResult<Product> { IsSuccess = false, Message = "Category Not Found" };

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

                Images = model.ImageUrls?.Select(url => new ProductImage
                {
                    Url = url
                }).ToList() ?? new List<ProductImage>()
            };
            await _uow.Products.CreateAsync(product);
            await _uow.SaveChangesAsync();

            _cache.Remove(ProductCacheKey);

            return new ServiceResult<Product> { IsSuccess = true, Data = product };
        }

        public async Task<ServiceResult<Product>> UpdateAsync(UpdateProductDTO model)
        {
            var product = await _uow.Products.GetByIdAsync(model.Id);
            if (product == null) return new ServiceResult<Product> { IsSuccess = false, Message = "Not Exist" };

            product.UpdateFromDto(model);
            _uow.Products.Update(product);
            await _uow.SaveChangesAsync();

            _cache.Remove(ProductCacheKey);

            return new ServiceResult<Product> { IsSuccess = true, Data = product };
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var product = await _uow.Products.GetByIdAsync(id);
            if (product == null) return new ServiceResult<bool> { IsSuccess = false };

            _uow.Products.Delete(product);
            await _uow.SaveChangesAsync();

            _cache.Remove(ProductCacheKey);

            return new ServiceResult<bool> { IsSuccess = true };
        }
    }
}
