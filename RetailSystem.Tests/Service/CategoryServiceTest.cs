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
    public class CategoryServiceTest
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IBaseRepository<Category>> _repoMock;
        private readonly Mock<ILogger<CategoryService>> _loggerMock;
        private readonly IMemoryCache _cache;

        private readonly CategoryService _service;

        public CategoryServiceTest()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _repoMock = new Mock<IBaseRepository<Category>>();
            _loggerMock = new Mock<ILogger<CategoryService>>();

            _uowMock.Setup(x => x.Categories).Returns(_repoMock.Object);

            _cache = new MemoryCache(new MemoryCacheOptions());

            _service = new CategoryService(_loggerMock.Object, _cache, _uowMock.Object);
        }

        // Test GetAllAsync when cache miss, it should fetch from DB and set cache
        [Fact]
        public async Task GetAll_Should_Fetch_From_DB_When_Cache_Miss()
        {
            // Arrange
            var data = new List<Category>
            {
                new Category { Id = 1, Name = "C1" }
            };

            _repoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(data);

            // Act ( 1st time - cache miss)
            var result1 = await _service.GetAllAsync();

            // Assert
            result1.Data.Should().HaveCount(1);

            // Verify DB called
            _repoMock.Verify(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<string>()
                ), Times.Once);

            // Act (2nd time - cache hit)
            var result2 = await _service.GetAllAsync();

            result2.Data.Should().HaveCount(1);

            // DB not recalled
            _repoMock.Verify(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<string>()
                ), Times.Once);
        }

        // Test CreateAsync when valid, it should create category and clear cache
        [Fact]
        public async Task Create_Should_Clear_Cache()
        {
            // Arrange
            _repoMock.Setup(x => x.CreateAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            _uowMock.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // set cache
            _cache.Set("AllCategories", new List<Category>());

            // Act
            await _service.CreateAsync(new CreateCategoryDTO
            {
                Name = "New"
            });

            // cache have to be remove
            _cache.TryGetValue("AllCategories", out _).Should().BeFalse();
        }

        // Test GetAllAsync when cache hit, it should return from cache and not call DB
        [Fact]
        public async Task GetAll_Should_Return_From_Cache()
        {
            var cacheKey = "AllCategories";

            _cache.Set(cacheKey, new List<Category>
            {
                new Category { Id = 1 }
            });

            var result = await _service.GetAllAsync();

            result.Data.Should().HaveCount(1);

            _repoMock.Verify(x => x.GetAllAsync(null, null), Times.Never);
        }

        // Test CreateAsync when name is empty, it should throw "Name is required"
        [Fact]
        public async Task Create_Should_Throw_When_Name_Invalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(new CreateCategoryDTO { Name = "" }));
        }

        // Test CreateAsync when valid, it should create category and return success
        [Fact]
        public async Task Create_Should_Success()
        {
            _repoMock.Setup(x => x.CreateAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CreateAsync(new CreateCategoryDTO
            {
                Name = "New",
                Description = "Test"
            });

            result.IsSuccess.Should().BeTrue();

            _repoMock.Verify(x => x.CreateAsync(It.IsAny<Category>()), Times.Once);
            _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        // Test GetByIdAsync when category exists, it should return category
        [Fact]
        public async Task GetById_Should_Return_Fail_When_Not_Found()
        {
            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Category)null);

            var result = await _service.GetByIdAsync(1);

            result.IsSuccess.Should().BeFalse();
        }

        // Test GetByIdAsync when category exists, it should return category and set cache
        [Fact]
        public async Task GetById_Should_Return_Data_And_Cache()
        {
            var category = new Category { Id = 1 };

            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(category);

            var result = await _service.GetByIdAsync(1);

            result.IsSuccess.Should().BeTrue();

            // call second time to verify cache
            var result2 = await _service.GetByIdAsync(1);

            _repoMock.Verify(x => x.GetByIdAsync(1), Times.Once);
        }

        // Test UpdateAsync when category not exists, it should return failure
        [Fact]
        public async Task Update_Should_Fail_When_Not_Exist()
        {
            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Category)null);

            var result = await _service.UpdateAsync(1, new UpdateCategoryDTO());

            result.IsSuccess.Should().BeFalse();
        }

        // Test UpdateAsync when valid, it should update category and return success
        [Fact]
        public async Task Update_Should_Success()
        {
            var category = new Category { Id = 1, Name = "Old" };

            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(category);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.UpdateAsync(1, new UpdateCategoryDTO
            {
                Name = "New",
                Description = "Updated"
            });

            result.IsSuccess.Should().BeTrue();
            category.Name.Should().Be("New");
            category.Description.Should().Be("Updated");

            _repoMock.Verify(x => x.Update(category), Times.Once);
        }


    }
}
