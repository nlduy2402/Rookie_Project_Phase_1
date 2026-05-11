using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.ResponseModels;
using RetailSystem.Infrastructure.Services.Interfaces;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace RetailSystem.Tests.Service
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly ProductService _productService;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        public ProductServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<ProductService>>();
            _cacheMock = new Mock<IMemoryCache>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
            _productRepoMock = new Mock<IProductRepository>();
            _cacheEntryMock = new Mock<ICacheEntry>();
            _categoryRepoMock = new Mock<ICategoryRepository>();

            _cacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(_cacheEntryMock.Object);
                    _uowMock.Setup(x => x.Products).Returns(_productRepoMock.Object);

            _productService = new ProductService(
                _loggerMock.Object,
                _cacheMock.Object,
                _uowMock.Object,
                _cloudinaryServiceMock.Object);
        }

        [Fact]
        public async Task GetAllProductAsync_WhenCacheExists_ShouldReturnFromCache()
        {
            // Arrange
            var cachedProducts = new List<Product> { new Product { Id = 1, Name = "Cached Product" } };
            object outValue = cachedProducts;

            // mock cache have data
            _cacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(true);

            // Act
            var result = await _productService.GetAllProductAsync();

            // Assert (Kiểm tra)
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().Name.Should().Be("Cached Product");

            // Ensure doesn't call DB 
            _uowMock.Verify(u => u.Products.GetAllAsync(null, null), Times.Never);
        }

        [Fact]
        // Test GetAllProductAsync when cache miss, it should fetch from DB and set cache
        public async Task GetAllProductAsync_WhenCacheEmpty_ShouldReturnFromDbAndSetCache()
        {
            // Arrange
            object? outValue = null;
            var dbProducts = new List<Product>
            {
                new Product { Id = 1, Name = "DB Product" }
            };
            // Giả lập cache trống (TryGetValue trả về false)
            _cacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out outValue))
                .Returns(false);

            // Giả lập DB trả về dữ liệu
            _uowMock.Setup(u => u.Products.GetAllAsync(null, "Images,Category"))
                    .ReturnsAsync(dbProducts);

            // Giả lập việc tạo Cache Entry (vì IMemoryCache.Set gọi CreateEntry nội bộ)
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                      .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _productService.GetAllProductAsync();
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().Name.Should().Be("DB Product");

            // Verify call DB once
            _uowMock.Verify(u => u.Products.GetAllAsync(null, "Images,Category"), Times.Once);
        }

        //public async Task<ServiceResult<Product>> GetProductByIdAsync(int id)
        //{
        //    if (id <= 0) return new ServiceResult<Product> { IsSuccess = false, Message = "Invalid Product Id" };

        //    var product = await _uow.Products.GetFirstOrDefaultAsync(p => p.Id == id, "Images,Category");

        //    if (product == null) return new ServiceResult<Product> { IsSuccess = false, Message = "Product Not Exist" };

        //    return new ServiceResult<Product> { IsSuccess = true, Data = product };
        //}
        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            int productId = 1;
            var product = new Product { Id = productId, Name = "Test Product" };
            _uowMock.Setup(u => u.Products.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                "Images,Category"))
                .ReturnsAsync(product);
            // Act
            var result = await _productService.GetProductByIdAsync(productId);
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(productId);
            result.Data.Name.Should().Be("Test Product");
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductNotExists_ShouldReturnErrorMessage()
        {
            int productId = 1;
            var product = new Product { Id = 2, Name = "Test Product" };
            _uowMock.Setup(u => u.Products.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                "Images,Category"))
                .ReturnsAsync((Product)null);

            var result = await _productService.GetProductByIdAsync(productId);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Product Not Exist");
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenIdInvalid_ShouldReturnErrorMessage()
        {
            int productId = -1;
            var result = await _productService.GetProductByIdAsync(productId);
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid Product Id");
        }

        [Fact]
        public async Task GetProductByIdWithReviewAsync_WhenIdInvalid_ShouldReturnErrorMessage()
        {
            int productId = -1;
            var result = await _productService.GetProductByIdWithReviewAsync(productId);
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid Product Id");

            _uowMock.Verify(
                x => x.Products.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task GetProductByIdWithReviewAsync_WhenProductNotExists_ShouldReturnErrorMessage()
        {
            int productId = 1;
            _uowMock.Setup(u => u.Products.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                "Images,Category,Reviews"))
                .ReturnsAsync((Product)null);

            var result = await _productService.GetProductByIdWithReviewAsync(productId);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Product Not Exist");
            _uowMock.Verify(
                x => x.Products.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProductByIdWithReviewAsync_NoReviews_ShouldReturnAverageZero()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "IPhone",
                Images = new List<ProductImage>
                {
                    new ProductImage { Url = "img1.jpg" }
                }
            };

            _uowMock.Setup(x => x.Products.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(product);

            _uowMock.Setup(x => x.Reviews.GetByProductIdAsync(product.Id))
                .ReturnsAsync(new List<Review>());

            // Act
            var result = await _productService.GetProductByIdWithReviewAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.AverageRating.Should().Be(0);
            result.Data.TotalReviews.Should().Be(0);

            result.Data.Name.Should().Be("IPhone");

            result.Data.Images.Should().HaveCount(1);
            result.Data.Images.First().Should().Be("img1.jpg");
        }

        [Fact]
        public async Task GetProductByIdWithReviewAsync_WhenProductExists_ShouldReturnProductWithReviews()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Images = new List<ProductImage>
                {
                    new ProductImage { Url = "img1.jpg" },
                    new ProductImage { Url = "img2.jpg" }
                }

            };
            var Reviews = new List<Review>
            {
                new Review { ProductId = 1, Rating = 5, Comment = "Great!" },
                new Review { ProductId = 1, Rating = 4, Comment = "Good!" }
            };
            _uowMock.Setup(u => u.Products.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                "Images,Category"))
                .ReturnsAsync(product);
            _uowMock.Setup(u => u.Reviews.GetByProductIdAsync(It.IsAny<int>())).ReturnsAsync(Reviews);

            // Act
            var result = await _productService.GetProductByIdWithReviewAsync(product.Id);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Name.Should().Be("Test Product");
            result.Data.Images.Count.Should().Be(2);
            result.Data.AverageRating.Should().Be(4.5);
            result.Data.Reviews.Count.Should().Be(2);
            result.Data.TotalReviews.Should().Be(2);



        }

        [Fact]
        public async Task GetProductByIdWithReviewAsync_ShouldCallRepositoryWithCorrectInclude()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>()
            };

            _uowMock.Setup(x => x.Products.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"
                ))
                .ReturnsAsync(product);

            _uowMock.Setup(x => x.Reviews.GetByProductIdAsync(1))
                .ReturnsAsync(new List<Review>());

            // Act
            await _productService.GetProductByIdWithReviewAsync(1);

            // Assert
            _uowMock.Verify(x => x.Products.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByCategory_WhenInvalidId_ShouldReturnErrorMessage()
        {
            var id = 0;
            var result = await _productService.GetByCategory(id);
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Be("Invalid Input.");
            _uowMock.Verify(
                x => x.Products.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task GetByCategory_WhenCacheExists_ShouldReturnFromCache()
        {
            // Arrange
            var cachedProducts = new List<Product>
            {
                new Product { Id = 1, Name = "IPhone" }
            };

            object cacheValue = cachedProducts;

            _cacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(true);

            // Act
            var result = await _productService.GetByCategory(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Should().HaveCount(1);
            result.Data.First().Name.Should().Be("IPhone");

            _uowMock.Verify(
                x => x.Products.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task GetByCategory_WhenCacheMiss_ShouldFetchFromDatabase()
        {
            // Arrange
            object cacheValue = null;

            _cacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop"
                }
            };

            _productRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"
                ))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetByCategory(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Should().HaveCount(1);

            _productRepoMock.Verify(
                x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByCategory_WhenNoProducts_ShouldReturnEmptyMessage()
        {
            // Arrange
            object cacheValue = null;

            _cacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            _productRepoMock.Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(new List<Product>());

            // Act
            var result = await _productService.GetByCategory(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Message.Should().Be("No Product Found in This Category!");

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCategory_WhenCacheMiss_ShouldSetCache()
        {
            // Arrange
            object cacheValue = null;

            _cacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "PC"
                }
            };

            _productRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(products);

            // Act
            await _productService.GetByCategory(1);

            // Assert
            _cacheMock.Verify(
                x => x.CreateEntry(It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByCategory_WhenCacheMiss_ShouldLogInformation()
        {
            // Arrange
            object cacheValue = null;

            _cacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            _productRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(new List<Product>());

            // Act
            await _productService.GetByCategory(1);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Cache miss")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByCategory_ShouldCallRepositoryWithCorrectInclude()
        {
            // Arrange
            object cacheValue = null;

            _cacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            _productRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"
                ))
                .ReturnsAsync(new List<Product>());

            // Act
            await _productService.GetByCategory(2);

            // Assert
            _productRepoMock.Verify(
                x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateAsync_WhenCategoryNotFound_ShouldReturnFail()
        {
            // Arrange
            var dto = new CreateProductDTO
            {
                Name = "Laptop",
                CategoryId = 1
            };

            _uowMock
                .Setup(x => x.Categories.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _productService.CreateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Category Not Found");

            // Không create product
            _productRepoMock.Verify(
                x => x.CreateAsync(It.IsAny<Product>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CreateAsync_WithoutImages_ShouldCreateSuccessfully()
        {
            // Arrange
            var dto = new CreateProductDTO
            {
                Name = "Laptop",
                Description = "Gaming",
                Price = 1000,
                Quantity = 5,
                CategoryId = 1
            };

            _uowMock
                .Setup(x => x.Categories.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync(new Category());

            // Act
            var result = await _productService.CreateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Name.Should().Be("Laptop");
            result.Data.Price.Should().Be(1000);

            result.Data.Images.Should().BeEmpty();

            _productRepoMock.Verify(
                x => x.CreateAsync(It.IsAny<Product>()),
                Times.Once
            );

            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateAsync_WithImages_ShouldUploadAndAddImages()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();

            var dto = new CreateProductDTO
            {
                Name = "Laptop",
                CategoryId = 1,
                Images = new List<IFormFile>
        {
            fileMock.Object
        }
            };

           _uowMock
                .Setup(x => x.Categories.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync(new Category());

            var uploadResult = new ImageUploadResult
            {
                PublicId = "abc123",
                SecureUrl = new Uri("https://test.com/image.jpg")
            };

            _cloudinaryServiceMock
                .Setup(x => x.AddPhotoAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(uploadResult);

            // Act
            var result = await _productService.CreateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Images.Should().HaveCount(1);

            result.Data.Images.First().Url
                .Should().Be("https://test.com/image.jpg");

            result.Data.Images.First().Name
                .Should().Be("abc123");
        }

        [Fact]
        public async Task CreateAsync_WhenUploadFails_ShouldNotAddImage()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();

            var dto = new CreateProductDTO
            {
                Name = "Laptop",
                CategoryId = 1,
                Images = new List<IFormFile>
        {
            fileMock.Object
        }
            };

            _uowMock
                .Setup(x => x.Categories.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync(new Category());

            var uploadResult = new ImageUploadResult
            {
                Error = new Error
                {
                    Message = "Upload failed"
                }
            };

            _cloudinaryServiceMock
                .Setup(x => x.AddPhotoAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(uploadResult);

            // Act
            var result = await _productService.CreateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Images.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_ShouldRemoveCache()
        {
            // Arrange
            var dto = new CreateProductDTO
            {
                Name = "Laptop",
                CategoryId = 1
            };

            _uowMock
                .Setup(x => x.Categories.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync(new Category());

            // Act
            await _productService.CreateAsync(dto);

            // Assert
            _cacheMock.Verify(
                x => x.Remove(It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldCallSaveChanges()
        {
            // Arrange
            var dto = new CreateProductDTO
            {
                Name = "Laptop",
                CategoryId = 1
            };

            _uowMock
                .Setup(x => x.Categories.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync(new Category());

            // Act
            await _productService.CreateAsync(dto);

            // Assert
            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateProductWithCorrectData()
        {
            // Arrange
            var dto = new CreateProductDTO
            {
                Name = "Laptop",
                Description = "Gaming Laptop",
                Price = 2000,
                Quantity = 10,
                RAM = "16GB",
                SSD = "512GB",
                ChipSet = "Intel",
                CategoryId = 1
            };

            _uowMock
                .Setup(x => x.Categories.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync(new Category());

            Product capturedProduct = null;

            _productRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Product>()))
                .Callback<Product>(p => capturedProduct = p);

            // Act
            await _productService.CreateAsync(dto);

            // Assert
            capturedProduct.Should().NotBeNull();

            capturedProduct.Name.Should().Be(dto.Name);
            capturedProduct.Price.Should().Be(dto.Price);
            capturedProduct.RAM.Should().Be(dto.RAM);
            capturedProduct.SSD.Should().Be(dto.SSD);
        }

        [Fact]
        public async Task UpdateAsync_WhenProductNotFound_ShouldReturnFail()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync((Product)null);

            var dto = new UpdateProductDTO
            {
                Id = 1
            };

            // Act
            var result = await _productService.UpdateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Not Exist");
        }

        [Fact]
        public async Task UpdateAsync_ShouldRemoveDeletedImages()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>
        {
            new ProductImage
            {
                Id = 1,
                Name = "img1"
            },
            new ProductImage
            {
                Id = 2,
                Name = "img2"
            }
        }
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            var dto = new UpdateProductDTO
            {
                Id = 1,
                ExistImages = new List<int> { 1 }
            };

            // Act
            var result = await _productService.UpdateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            product.Images.Should().HaveCount(1);

            product.Images.First().Id.Should().Be(1);

            _cloudinaryServiceMock.Verify(
                x => x.DeletePhotoAsync("img2"),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotDeleteCloudinary_WhenImageNameNull()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>
        {
            new ProductImage
            {
                Id = 2,
                Name = null
            }
        }
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            var dto = new UpdateProductDTO
            {
                Id = 1,
                ExistImages = new List<int>()
            };

            // Act
            await _productService.UpdateAsync(dto);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.DeletePhotoAsync(It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldUploadNewImages()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>()
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            var fileMock = new Mock<IFormFile>();

            var dto = new UpdateProductDTO
            {
                Id = 1,
                ExistImages = new List<int>(),
                Images = new List<IFormFile>
                {
                    fileMock.Object
                }
            };

            _cloudinaryServiceMock
                .Setup(x => x.AddPhotoAsync(
                    It.IsAny<IFormFile>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    PublicId = "abc",
                    SecureUrl = new Uri("https://img.com/a.jpg")
                });

            // Act
            var result = await _productService.UpdateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            product.Images.Should().HaveCount(1);

            product.Images.First().Name.Should().Be("abc");

            product.Images.First().Url
                .Should().Be("https://img.com/a.jpg");

            _cloudinaryServiceMock.Verify(
                x => x.AddPhotoAsync(
                    It.IsAny<IFormFile>()),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateAsync_WhenImagesNull_ShouldNotUpload()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>()
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            var dto = new UpdateProductDTO
            {
                Id = 1,
                ExistImages = new List<int>(),
                Images = null
            };

            // Act
            await _productService.UpdateAsync(dto);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.AddPhotoAsync(It.IsAny<IFormFile>()),
                Times.Never
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallUpdateAndSaveChanges()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>()
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var dto = new UpdateProductDTO
            {
                Id = 1,
                ExistImages = new List<int>()
            };

            // Act
            var result = await _productService.UpdateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _productRepoMock.Verify(
                x => x.Update(product),
                Times.Once
            );

            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteAsync_WhenProductNotFound_ShouldReturnFail()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.DeleteAsync(1);

            // Assert
            result.IsSuccess.Should().BeFalse();

            result.Message.Should()
                .Be("Product do not exist!");
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteProductSuccessfully()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>()
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _productService.DeleteAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Message.Should()
                .Be("Product Deleted !");

            _productRepoMock.Verify(
                x => x.Delete(product),
                Times.Once
            );

            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCloudinaryImages()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>
        {
            new ProductImage
            {
                Name = "img1"
            },
            new ProductImage
            {
                Name = "img2"
            }
        }
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _productService.DeleteAsync(1);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.DeletePhotoAsync("img1"),
                Times.Once
            );

            _cloudinaryServiceMock.Verify(
                x => x.DeletePhotoAsync("img2"),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteAsync_WhenImageNameNull_ShouldNotCallCloudinary()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>
        {
            new ProductImage
            {
                Name = null
            }
        }
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _productService.DeleteAsync(1);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.DeletePhotoAsync(
                    It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task DeleteAsync_WhenImagesNull_ShouldDeleteSuccessfully()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = null
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _productService.DeleteAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _cloudinaryServiceMock.Verify(
                x => x.DeletePhotoAsync(
                    It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task DeleteAsync_WhenSaveChangesThrowsException_ShouldThrow()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Images = new List<ProductImage>()
            };

            _productRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images"))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var result = await _productService.DeleteAsync(1);

            // Assert
            result.IsSuccess.Should().BeFalse();

            result.Message.Should()
                .Be("Error occured while update data");
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop"
                },
                new Product
                {
                    Id = 2,
                    Name = "Phone"
                }
            };

            _productRepoMock
                .Setup(x => x.GetPagedAsync(1, 5))
                .ReturnsAsync((products, 10));

            // Act
            var result = await _productService.GetPagedAsync(1, 5);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Should().NotBeNull();

            result.Data.Items.Should().HaveCount(2);

            result.Data.TotalCount.Should().Be(10);

            result.Data.Page.Should().Be(1);

            result.Data.PageSize.Should().Be(5);

            result.Data.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetPagedAsync_WhenNoProducts_ShouldReturnEmptyList()
        {
            // Arrange
            var products = new List<Product>();

            _productRepoMock
                .Setup(x => x.GetPagedAsync(1, 5))
                .ReturnsAsync((products, 0));

            // Act
            var result = await _productService.GetPagedAsync(1, 5);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Should().NotBeNull();

            result.Data.Items.Should().BeEmpty();

            result.Data.TotalCount.Should().Be(0);

            result.Data.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldCallRepository()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetPagedAsync(1, 5))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            await _productService.GetPagedAsync(1, 5);

            // Assert
            _productRepoMock.Verify(
                x => x.GetPagedAsync(1, 5),
                Times.Once
            );
        }

        [Fact]
        public async Task GetTopSellingProductCardsAsync_ShouldReturnData()
        {
            // Arrange
            var topDtos = new List<TopProductDTO>
            {
                new TopProductDTO { ProductId = 1, TotalSold = 10 }
            };

                    var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
                    Images = new List<ProductImage>(),
                    Category = new Category()
                }
            };

            _productRepoMock
                .Setup(x => x.GetTopSellingProductsAsync(2, 4))
                .ReturnsAsync(topDtos);

            _productRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetTopSellingProductCardsAsync(2);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Should().NotBeNull();

            result.Data.First().TotalSold.Should().Be(10);
        }

        [Fact]
        public async Task GetTopSellingProductCardsAsync_WhenProductNotFound_ShouldFilterOut()
        {
            // Arrange
            var topDtos = new List<TopProductDTO>
            {
                new TopProductDTO { ProductId = 1, TotalSold = 5 }
            };

            _productRepoMock
                .Setup(x => x.GetTopSellingProductsAsync(1, 4))
                .ReturnsAsync(topDtos);

            _productRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"))
                .ReturnsAsync(new List<Product>());

            // Act
            var result = await _productService.GetTopSellingProductCardsAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTopSellingProductCardsAsync_WhenException_ShouldReturnFail()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetTopSellingProductsAsync(It.IsAny<int>(), 4))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _productService.GetTopSellingProductCardsAsync(5);

            // Assert
            result.IsSuccess.Should().BeFalse();

            result.Message.Should()
                .Be("An error occurred while fetching top selling products.");
        }

        [Fact]
        public async Task GetTopSellingProductCardsAsync_ShouldMapTotalSold()
        {
            // Arrange
            var dto = new List<TopProductDTO>
            {
                new TopProductDTO { ProductId = 1, TotalSold = 99 }
            };

            var product = new Product
            {
                Id = 1,
                Name = "Test",
                Images = new List<ProductImage>(),
                Category = new Category()
            };

            _productRepoMock
                .Setup(x => x.GetTopSellingProductsAsync(1, 4))
                .ReturnsAsync(dto);

            _productRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    "Images,Category"))
                .ReturnsAsync(new List<Product> { product });

            // Act
            var result = await _productService.GetTopSellingProductCardsAsync(1);

            // Assert
            result.Data.First().TotalSold.Should().Be(99);
        }
    }
}