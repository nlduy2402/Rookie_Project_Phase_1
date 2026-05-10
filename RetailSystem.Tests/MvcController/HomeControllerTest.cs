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
using FluentAssertions;
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
        public async Task Index_ShouldCallGetTopSellingProducts()
        {
            // Arrange
            _mockProductService
                .Setup(x => x.GetTopSellingProductCardsAsync(4))
                .ReturnsAsync(new ServiceResult<IEnumerable<ProductViewModel>>
                {
                    IsSuccess = true,
                    Data = new List<ProductViewModel>()
                });

            // Act
            await _controller.Index();

            // Assert
            _mockProductService.Verify(
                x => x.GetTopSellingProductCardsAsync(4),
                Times.Once
            );
        }

        [Fact]
        public async Task Index_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var serviceResult = new ServiceResult<IEnumerable<ProductViewModel>>
            {
                IsSuccess = false,
                Data = null 
            };

            _mockProductService
                .Setup(s => s.GetTopSellingProductCardsAsync(4))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsEmptyList_WhenDataIsNullButSuccess()
        {
            // Arrange
            var serviceResult = new ServiceResult<IEnumerable<ProductViewModel>>
            {
                IsSuccess = true,
                Data = null,
                Message = "Success"
            };

            _mockProductService
                .Setup(s => s.GetTopSellingProductCardsAsync(4))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsAssignableFrom<IEnumerable<ProductViewModel>>(
                viewResult.Model
            );

            Assert.Empty(model);
        }
    }
}
