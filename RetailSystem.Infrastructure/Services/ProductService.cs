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
using RetailSystem.Shared.ResponseModels;
using RetailSystem.Shared.ViewModels;
using Microsoft.Data.SqlClient;

namespace RetailSystem.Infrastructure.Services
{
    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly new IUnitOfWork _uow;
        private const string ProductCacheKey = "AllProducts";
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly ICloudinaryService _cloudinaryService;
        public ProductService(ILogger<ProductService> logger, IMemoryCache cache,IUnitOfWork uow, ICloudinaryService cloudinaryService) : base(uow, cache)
        {
            _logger = logger;
            _uow = uow;
            _cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            _cloudinaryService = cloudinaryService;
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
        public async Task<ServiceResult<ProductDetailViewModel>> GetProductByIdWithReviewAsync(int id)
        {
            var product = await _uow.Products.GetFirstOrDefaultAsync(
                p => p.Id == id,
                "Images,Category"
            );

            if (product == null)
            {
                return new ServiceResult<ProductDetailViewModel>
                {
                    IsSuccess = false,
                    Message = "Product Not Exist"
                };
            }

            // ⭐ LẤY REVIEW
            var reviews = await _uow.Reviews.GetByProductIdAsync(product.Id);

            // ⭐ MAP VM
            var vm = new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Images = product.Images.Select(x => x.Url).ToList(),

                // ⭐ rating
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any()
                    ? reviews.Average(r => r.Rating)
                    : 0,

                Reviews = reviews.Select(r => new ReviewViewModel
                {
                    Rating = r.Rating,
                    Comment = r.Comment,
                }).ToList()
            };

            return new ServiceResult<ProductDetailViewModel>
            {
                IsSuccess = true,
                Data = vm
            };
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
            //if (model.Images != null && model.Images.Any())
            //{
            //    Console.WriteLine($"Received {model.Images.Count} images for upload.");
            //    foreach (var file in model.Images)
            //    {
            //        var uploadResult = await _cloudinaryService.AddPhotoAsync(file);

            //        if (uploadResult.Error == null)
            //        {
            //            var productImage = new ProductImage
            //            {
            //                Url = uploadResult.SecureUrl.AbsoluteUri,
            //                Name = file.FileName,
            //            };
            //            product.Images.Add(productImage);
            //        }
            //    }
            //}
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
            if (product == null) return new ServiceResult<bool> { IsSuccess = false,Message="Product do not exist!" };

            try {
                _uow.Products.Delete(product);
                await _uow.SaveChangesAsync();

                _cache.Remove(ProductCacheKey);
                return new ServiceResult<bool> { IsSuccess = true, Message="Product Deleted !" };
            }
            catch(DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547))
                {
                    return new ServiceResult<bool> {IsSuccess=false, Message="Can not delete this product because it is exist in orders." };
                }

                return new ServiceResult<bool> { IsSuccess = false, Message = "Error occured while update data" };
            }
            
            //_uow.Products.Delete(product);
            //await _uow.SaveChangesAsync();

            //_cache.Remove(ProductCacheKey);

            //return new ServiceResult<bool> { IsSuccess = true };
        }

        public async Task<ServiceResult<PageResult<Product>>> GetPagedAsync(int page, int pageSize)
        {
            var (items, totalCount) = await _uow.Products.GetPagedAsync(page, pageSize);

            return new ServiceResult<PageResult<Product>> {
                IsSuccess = true,
                Data = new PageResult<Product>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                }
            };
        
        }
    }
}
