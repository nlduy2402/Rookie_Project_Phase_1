using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.Service
{
    public class TestEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class TestService : BaseService<TestEntity>
    {
        private readonly IBaseRepository<TestEntity> _repo;

        public TestService(
            IUnitOfWork uow,
            IMemoryCache cache,
            IBaseRepository<TestEntity> repo)
            : base(uow, cache)
        {
            _repo = repo;
        }

        protected override IBaseRepository<TestEntity> GetRepository()
        {
            return _repo;
        }
    }
    public class BaseServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;

        private readonly Mock<IBaseRepository<TestEntity>>
            _repoMock;

        private readonly IMemoryCache _cache;

        private readonly TestService _service;

        public BaseServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();

            _repoMock =
                new Mock<IBaseRepository<TestEntity>>();

            _cache = new MemoryCache(
                new MemoryCacheOptions());

            _service = new TestService(
                _uowMock.Object,
                _cache,
                _repoMock.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_WhenCacheMiss_ShouldFetchFromDatabase()
        {
            // Arrange
            var data = new List<TestEntity>
        {
            new TestEntity
            {
                Id = 1,
                Name = "Product 1"
            }
        };

            _repoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<TestEntity, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(data);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);

            result.First().Name.Should().Be("Product 1");

            _repoMock.Verify(
                x => x.GetAllAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(),
                    It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllAsync_WhenCacheExists_ShouldReturnCache()
        {
            // Arrange
            var cached = new List<TestEntity>
        {
            new TestEntity
            {
                Id = 1,
                Name = "Cached"
            }
        };

            _cache.Set("TestEntity_All", cached);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);

            result.First().Name.Should().Be("Cached");

            _repoMock.Verify(
                x => x.GetAllAsync(null, null),
                Times.Never
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheMiss_ShouldFetchFromDatabase()
        {
            // Arrange
            var entity = new TestEntity
            {
                Id = 1,
                Name = "Laptop"
            };

            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(entity);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();

            result.Name.Should().Be("Laptop");

            _repoMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheExists_ShouldReturnCache()
        {
            // Arrange
            var entity = new TestEntity
            {
                Id = 1,
                Name = "Cached Entity"
            };

            _cache.Set("TestEntity_1", entity);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();

            result.Name.Should().Be("Cached Entity");

            _repoMock.Verify(
                x => x.GetByIdAsync(1),
                Times.Never
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenEntityNotFound_ShouldReturnNull()
        {
            // Arrange
            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((TestEntity)null);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateEntityAndClearCache()
        {
            // Arrange
            var entity = new TestEntity
            {
                Id = 1,
                Name = "New Product"
            };

            _repoMock
                .Setup(x => x.CreateAsync(entity))
                .Returns(Task.CompletedTask);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // cache fake
            _cache.Set("TestEntity_All",
                new List<TestEntity>());

            // Act
            var result = await _service.CreateAsync(entity);

            // Assert
            result.Should().NotBeNull();

            result.Name.Should().Be("New Product");

            _repoMock.Verify(
                x => x.CreateAsync(entity),
                Times.Once
            );

            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );

            // cache phải bị remove
            _cache.TryGetValue(
                "TestEntity_All",
                out List<TestEntity>? cacheResult
            ).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityNotFound_ShouldReturnMessage()
        {
            // Arrange
            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((TestEntity)null);

            // Act
            var result = await _service.DeleteAsync(1);

            // Assert
            result.Should()
                .Be("Not exist data to delete!");

            _repoMock.Verify(
                x => x.Delete(It.IsAny<TestEntity>()),
                Times.Never
            );
        }

        [Fact]
        public async Task DeleteAsync_WhenSuccess_ShouldDeleteAndClearCache()
        {
            // Arrange
            var entity = new TestEntity
            {
                Id = 1,
                Name = "Delete Product"
            };

            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            _cache.Set("TestEntity_All",
                new List<TestEntity>());

            _cache.Set("TestEntity_1",
                entity);

            // Act
            var result = await _service.DeleteAsync(1);

            // Assert
            result.Should().Be("Deleted!");

            _repoMock.Verify(
                x => x.Delete(entity),
                Times.Once
            );

            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );

            _cache.TryGetValue(
                "TestEntity_All",
                out List<TestEntity>? allCache
            ).Should().BeFalse();

            _cache.TryGetValue(
                "TestEntity_1",
                out TestEntity? entityCache
            ).Should().BeFalse();
        }
    }
}
