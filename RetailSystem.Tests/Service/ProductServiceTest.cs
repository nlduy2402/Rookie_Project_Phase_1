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

namespace RetailSystem.Tests.Service
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<ProductService>> _loggerMock;

        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _cacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<ProductService>>();

            _service = new ProductService(
                _loggerMock.Object,
                _cacheMock.Object,
                _uowMock.Object
            );
        }


        // Test GetAllProductAsync when cache miss, it should fetch from DB and set cache
        [Fact]
        public async Task GetAllProduct_Should_Fetch_From_DB_When_Cache_Miss()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "A" }
            };

            object cacheValue = null;

            _cacheMock
                .Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            _uowMock.Setup(x => x.Products.GetAllAsync(null, "Images,Category"))
                .ReturnsAsync(products);

            // Setup cache set 
            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock
                .Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            // Act
            var result = await _service.GetAllProductAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);

            _uowMock.Verify(x => x.Products.GetAllAsync(null, "Images,Category"), Times.Once);
        }

        // Test GetAllProductAsync when cache hit, it should return from cache and not call DB
        [Fact]
        public async Task GetAllProduct_Should_Return_From_Cache_When_Hit()
        {
            // Arrange
            var cachedProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Cached" }
            };

            object cacheValue = cachedProducts;

            _cacheMock
                .Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(true);

            // Act
            var result = await _service.GetAllProductAsync();

            // Assert
            result?.Data?.First().Name.Should().Be("Cached");

            _uowMock.Verify(x => x.Products.GetAllAsync(null, It.IsAny<string>()), Times.Never);
        }

        // Test GetProductByIdAsync when product not exists, it should return "Product Not Exist"
        [Fact]
        public async Task GetProductById_Should_Return_Fail_When_Not_Exist()
        {
            _uowMock.Setup(x => x.Products.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>(), "Images,Category"))
                .ReturnsAsync((Product)null);

            var result = await _service.GetProductByIdAsync(1);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Product Not Exist");
        }

        // Test GetProductByIdAsync when product exists, it should return product
        [Fact]
        public async Task GetProductById_Should_Return_Product()
        {
            var product = new Product { Id = 1 };

            _uowMock.Setup(x => x.Products.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>(), "Images,Category"))
                .ReturnsAsync(product);

            var result = await _service.GetProductByIdAsync(1);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        // Test CreateAsync when category not exist, it should return fail
        [Fact]
        public async Task Create_Should_Fail_When_Category_Not_Exist()
        {
            _uowMock.Setup(x => x.Categories.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category)null);

            var dto = new CreateProductDTO
            {
                CategoryId = 1,
                ImageUrls = new List<string>()
            };

            var result = await _service.CreateAsync(dto);

            result.IsSuccess.Should().BeFalse();
        }

        // Test CreateAsync when category exist, it should create product and remove cache
        [Fact]
        public async Task Create_Should_Success()
        {
            // Arrange
            var productRepoMock = new Mock<IBaseRepository<Product>>();
            var categoryRepoMock = new Mock<IBaseRepository<Category>>();

            //_uowMock.Setup(x => x.Products).Returns(productRepoMock.Object);
            _uowMock.Setup(x => x.Categories).Returns(categoryRepoMock.Object);

            categoryRepoMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Category { Id = 1 });

            productRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask); 

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1); 

            _cacheMock
                .Setup(x => x.Remove(It.IsAny<object>())); 

            var dto = new CreateProductDTO
            {
                Name = "Test",
                CategoryId = 1,
                ImageUrls = new List<string> { "img1" }
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            productRepoMock.Verify(x => x.CreateAsync(It.IsAny<Product>()), Times.Once);
            _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _cacheMock.Verify(x => x.Remove(It.IsAny<object>()), Times.Once);
        }

        // Test UpdateAsync when product not exist, it should return fail
        [Fact]
        public async Task Update_Should_Fail_When_Not_Exist()
        {
            _uowMock.Setup(x => x.Products.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            var result = await _service.UpdateAsync(new UpdateProductDTO { Id = 1 });

            result.IsSuccess.Should().BeFalse();
        }

        // Test UpdateAsync when product exist, it should update product and save changes
        [Fact]
        public async Task Update_Should_Success()
        {
            var product = new Product { Id = 1 };

            _uowMock.Setup(x => x.Products.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _service.UpdateAsync(new UpdateProductDTO { Id = 1 });

            result.IsSuccess.Should().BeTrue();

            _uowMock.Verify(x => x.Products.Update(product), Times.Once);
            _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        // Test DeleteAsync when product not exist, it should return fail
        [Fact]
        public async Task Delete_Should_Fail_When_Not_Exist()
        {
            _uowMock.Setup(x => x.Products.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            var result = await _service.DeleteAsync(1);

            result.IsSuccess.Should().BeFalse();
        }

        // Test DeleteAsync when product exist, it should delete product, save changes and remove cache
        [Fact]
        public async Task Delete_Should_Success()
        {
            var product = new Product { Id = 1 };

            _uowMock.Setup(x => x.Products.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _service.DeleteAsync(1);

            result.IsSuccess.Should().BeTrue();

            _uowMock.Verify(x => x.Products.Delete(product), Times.Once);
            _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _cacheMock.Verify(x => x.Remove(It.IsAny<object>()), Times.Once);
        }
    }
}
