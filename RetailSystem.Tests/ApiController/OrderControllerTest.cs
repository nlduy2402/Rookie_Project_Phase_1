using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RetailSystem.API.Controllers;
using RetailSystem.API.Shared;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.ApiController
{
    public class OrderControllerTest
    {
        private readonly Mock<ILogger<OrdersController>> _loggerMock;
        private readonly Mock<IOrderService> _orderServiceMock;

        private readonly OrdersController _controller;

        public OrderControllerTest()
        {
            _loggerMock = new Mock<ILogger<OrdersController>>();

            _orderServiceMock = new Mock<IOrderService>();

            _controller = new OrdersController(
                _loggerMock.Object,
                _orderServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task GetAll_WhenPageInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.GetAll(0, 4);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Page and PageSize must > 0.", badRequest.Value);
        }

        [Fact]
        public async Task GetAll_WhenPageSizeInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.GetAll(1, 0);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Page and PageSize must > 0.", badRequest.Value);
        }

        [Fact]
        public async Task GetAll_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.GetAllOrdersPagedAsync(1, 4))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = false,
                    Message = "Error loading orders"
                });

            // Act
            var result = await _controller.GetAll(1, 4);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Error loading orders", badRequest.Value);
        }

        [Fact]
        public async Task GetAll_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var pageResult = new PageResult<Order>
            {
                Items = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    FullName = "Nguyen Van A"
                }
            },
                TotalCount = 1,
                Page = 1,
                PageSize = 4
            };

            _orderServiceMock
                .Setup(x => x.GetAllOrdersPagedAsync(1, 4))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            var result = await _controller.GetAll(1, 4);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<PageResult<Order>>>(okResult.Value);

            Assert.True(response.Success);

            Assert.NotNull(response.Data);

            Assert.Single(response.Data.Items);

            Assert.Equal(1, response.Data.TotalCount);
        }

        [Fact]
        public async Task GetAll_ShouldCallService()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.GetAllOrdersPagedAsync(1, 4))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = new PageResult<Order>
                    {
                        Items = new List<Order>()
                    }
                });

            // Act
            await _controller.GetAll(1, 4);

            // Assert
            _orderServiceMock.Verify(
                x => x.GetAllOrdersPagedAsync(1, 4),
                Times.Once
            );
        }

        [Fact]
        public async Task Ship_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.ShipOrderAsync(1))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = false,
                    Message = "Cannot ship order"
                });

            // Act
            var result = await _controller.Ship(1);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Cannot ship order", badRequest.Value);
        }

        [Fact]
        public async Task Ship_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.ShipOrderAsync(1))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = true,
                    Data = "Order shipped successfully"
                });

            // Act
            var result = await _controller.Ship(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<string>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Order shipped successfully", response.Data);
        }

        [Fact]
        public async Task Ship_ShouldCallService()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.ShipOrderAsync(1))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = true,
                    Data = "Success"
                });

            // Act
            await _controller.Ship(1);

            // Assert
            _orderServiceMock.Verify(
                x => x.ShipOrderAsync(1),
                Times.Once
            );
        }
    }
}
