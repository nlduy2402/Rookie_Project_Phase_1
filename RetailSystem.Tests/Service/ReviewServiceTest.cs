using Microsoft.Extensions.Caching.Memory;
using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.Service
{
    public class ReviewServiceTest
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<IReviewRepository> _reviewRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;

        private readonly ReviewService _service;

        public ReviewServiceTest()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _cacheMock = new Mock<IMemoryCache>();

            _reviewRepoMock = new Mock<IReviewRepository>();
            _orderRepoMock = new Mock<IOrderRepository>();

            _uowMock.Setup(x => x.Reviews).Returns(_reviewRepoMock.Object);
            _uowMock.Setup(x => x.Orders).Returns(_orderRepoMock.Object);

            _service = new ReviewService(_uowMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task IsReviewedAsync_ShouldReturnTrue_WhenExists()
        {
            _reviewRepoMock
                .Setup(x => x.ExistsAsync(1, 2, "user1"))
                .ReturnsAsync(true);

            var result = await _service.IsReviewedAsync(1, 2, "user1");

            Assert.True(result);
        }

        [Fact]
        public async Task GetProductReviewsAsync_ShouldReturnList()
        {
            var data = new List<Review>
            {
                new Review { ProductId = 1, Rating = 5 }
            };

            _reviewRepoMock
                .Setup(x => x.GetByProductIdAsync(1))
                .ReturnsAsync(data);

            var result = await _service.GetProductReviewsAsync(1);

            Assert.Single(result);
        }

        [Fact]
        public async Task CreateReviewsAsync_ShouldThrow_WhenOrderNotFound()
        {
            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync((Order)null);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateReviewsAsync(1, "user1", new List<ReviewItemDTO>()));
        }

        [Fact]
        public async Task CreateReviewsAsync_ShouldThrow_WhenUnauthorized()
        {
            var order = new Order
            {
                UserId = "otherUser",
                Status = OrderStatus.Completed,
                OrderDetails = new List<OrderDetail>()
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateReviewsAsync(1, "user1", new List<ReviewItemDTO>()));
        }

        [Fact]
        public async Task CreateReviewsAsync_ShouldThrow_WhenNotCompleted()
        {
            var order = new Order
            {
                UserId = "user1",
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>()
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateReviewsAsync(1, "user1", new List<ReviewItemDTO>()));
        }

        [Fact]
        public async Task CreateReviewsAsync_ShouldCreateReviews_WhenValid()
        {
            var order = new Order
            {
                UserId = "user1",
                Status = OrderStatus.Completed,
                OrderDetails = new List<OrderDetail>
                            {
                                new OrderDetail { ProductId = 10 }
                            }
            };

            var items = new List<ReviewItemDTO>
            {
                new ReviewItemDTO
                {
                    ProductId = 10,
                    Rating = 5,
                    Comment = "Good"
                }
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            _reviewRepoMock
                .Setup(x => x.ExistsAsync(10, 1, "user1"))
                .ReturnsAsync(false);

            _reviewRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Review>()))
                .Returns(Task.CompletedTask);

            _uowMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uowMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _uowMock.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);
            await _service.CreateReviewsAsync(1, "user1", items);

            _reviewRepoMock.Verify(x => x.CreateAsync(It.IsAny<Review>()), Times.Once);
            _uowMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }
    }
}
