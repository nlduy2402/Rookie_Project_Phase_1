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
using RetailSystem.Infrastructure.Repository;

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
            if(id <= 0) return new ServiceResult<Product> { IsSuccess = false, Message = "Invalid Product Id" };

            var product = await _uow.Products.GetFirstOrDefaultAsync(p => p.Id == id, "Images,Category");

            if (product == null) return new ServiceResult<Product> { IsSuccess = false, Message = "Product Not Exist" };

            return new ServiceResult<Product> { IsSuccess = true, Data = product };
        }
        public async Task<ServiceResult<ProductDetailViewModel>> GetProductByIdWithReviewAsync(int id)
        {
            if (id <= 0)
            {
                return new ServiceResult<ProductDetailViewModel>
                {
                    IsSuccess = false,
                    Message = "Invalid Product Id"
                };
            }
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
            if(id <= 0) return new ServiceResult<List<Product>>() { IsSuccess = false, Message="Invalid Input."};
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
            };
            if (model.Images != null && model.Images.Any())
            {
                Console.WriteLine($"Received {model.Images.Count} images for upload.");
                foreach (var file in model.Images)
                {
                    var uploadResult = await _cloudinaryService.AddPhotoAsync(file);

                    if (uploadResult.Error == null)
                    {
                        var productImage = new ProductImage
                        {
                            Url = uploadResult.SecureUrl.AbsoluteUri,
                            Name = uploadResult.PublicId,
                        };
                        product.Images.Add(productImage);
                    }
                }
            }
            await _uow.Products.CreateAsync(product);
            await _uow.SaveChangesAsync();

            _cache.Remove(ProductCacheKey);

            return new ServiceResult<Product> { IsSuccess = true, Data = product };
        }

        public async Task<ServiceResult<Product>> UpdateAsync(UpdateProductDTO model)
        {
            var product = await _uow.Products.GetFirstOrDefaultAsync(p => p.Id == model.Id, "Images");
            if (product == null) return new ServiceResult<Product> { IsSuccess = false, Message = "Not Exist" };

            var removeImages = product.Images
                .Where(x => !model.ExistImages.Contains(x.Id))
                .ToList();

            foreach (var img in removeImages)
            {
                if (!string.IsNullOrEmpty(img.Name))
                {
                    await _cloudinaryService.DeletePhotoAsync(img.Name);
                }  
                product.Images.Remove(img);
            }

            if (model.Images != null && model.Images.Any())
            {
                foreach (var file in model.Images)
                {
                    // upload cloudinary
                    var imageUrl = await _cloudinaryService.AddPhotoAsync(file);

                    product.Images.Add(new ProductImage
                    {
                        Name = imageUrl.PublicId,
                        Url = imageUrl.SecureUrl.AbsoluteUri
                    });
                }
            }
            product.UpdateFromDto(model);
            _uow.Products.Update(product);
            await _uow.SaveChangesAsync();

            _cache.Remove(ProductCacheKey);

            return new ServiceResult<Product> { IsSuccess = true, Data = product };
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var product = await _uow.Products.GetFirstOrDefaultAsync(p => p.Id == id,"Images");
            if (product == null) return new ServiceResult<bool> { IsSuccess = false,Message="Product do not exist!" };

            try {
                if (product.Images != null && product.Images.Any())
                {
                    foreach (var img in product.Images)
                    {
                        if (!string.IsNullOrEmpty(img.Name)) 
                        {
                            await _cloudinaryService.DeletePhotoAsync(img.Name);
                        }
                    }
                }

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

        public async Task<ServiceResult<IEnumerable<ProductViewModel>>> GetTopSellingProductCardsAsync(int top)
        {
            try
            {
                // 1. Lấy danh sách TopProductDto từ Repository (chỉ gồm ID và TotalSold)
                var topSellingDtos = await _uow.Products.GetTopSellingProductsAsync(top, 4);
                var productIds = topSellingDtos.Select(d => d.ProductId).ToList();

                // 2. Lấy thông tin chi tiết Product Entity từ Repo 
                // Dùng Include để lấy đầy đủ Images và Category cho hàm ToCardVM
                var products = await _uow.Products.GetAllAsync(
                    p => productIds.Contains(p.Id),
                    includeProperties: "Images,Category"
                );

                // 3. Map và trộn dữ liệu
                var viewModels = topSellingDtos.Select(dto =>
                {
                    var p = products.FirstOrDefault(x => x.Id == dto.ProductId); 
                    if (products == null) return null;

                    var vm = p.ToCardVM();

                    vm.TotalSold = dto.TotalSold;

                    return vm;
                }).Where(x => x != null).ToList();

                return new ServiceResult<IEnumerable<ProductViewModel>>
                {
                    IsSuccess = true,
                    Data = viewModels
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top selling products");
                return new ServiceResult<IEnumerable<ProductViewModel>>
                {
                    IsSuccess = false,
                    Message = "An error occurred while fetching top selling products."
                };
            }
        }
    }
}
