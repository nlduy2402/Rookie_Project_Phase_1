using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RetailSystem.CustomerSite.Controllers;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.ResponseModels;
namespace RetailSystem.Tests.MvcController
{
    public class HomeControllerTest
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly HomeController _controller;

        public HomeControllerTest()
        {
            _mockProductService = new Mock<IProductService>();
            var logger = new NullLogger<HomeController>();
            _controller = new HomeController(logger, _mockProductService.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithList_WhenServiceReturnsSuccess()
        {
            // Arrange
            var fakeProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop" },
                new Product { Id = 2, Name = "Smartphone" }
            };

            var serviceResult = new ServiceResult<List<Product>>
            {
                IsSuccess = true,
                Data = fakeProducts 
            };

            _mockProductService
                .Setup(s => s.GetAllProductAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ProductViewModel>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            _mockProductService.Verify(s => s.GetAllProductAsync(), Times.Once);
        }

        [Fact]
        public async Task Index_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var errorMessage = "Lỗi kết nối cơ sở dữ liệu";
            var serviceResult = new ServiceResult<List<Product>>
            {
                IsSuccess = false,
                Message = errorMessage,
                Data = null 
            };

            _mockProductService
                .Setup(s => s.GetAllProductAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task Index_ReturnsEmptyList_WhenDataIsNullButSuccess()
        {
            var serviceResult = new ServiceResult<List<Product>> { IsSuccess = true, Data = null, Message = "Success" };

            _mockProductService
                .Setup(s => s.GetAllProductAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ProductViewModel>>(viewResult.Model);
            Assert.Empty(model);
        }
    }
}
