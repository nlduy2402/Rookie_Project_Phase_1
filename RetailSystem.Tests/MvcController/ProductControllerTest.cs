using Microsoft.AspNetCore.Mvc;
using Moq;
using RetailSystem.CustomerSite.Controllers;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.ResponseModels;

namespace RetailSystem.Tests.MvcController
{
    public class ProductControllerTest
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly ProductController _controller;

        public ProductControllerTest()
        {
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            //_controller = new ProductController(_mockProductService.Object, _mockCategoryService.Object);
        }

        #region Index Tests
        [Fact]
        public async Task Index_ReturnsViewWithProducts_WhenSuccess()
        {
            // Arrange
            var products = new List<Product> { new Product { Id = 1, Name = "P1" } };
            var serviceResult = new ServiceResult<List<Product>>
            {
                IsSuccess = true,
                Data = products
            };

            _mockProductService.Setup(s => s.GetAllProductAsync()).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<List<ProductViewModel>>(viewResult.Model);
        }

        [Fact]
        public async Task Index_ReturnsBadRequest_WhenFailure()
        {
            // Arrange
            var serviceResult = new ServiceResult<List<Product>>
            {
                IsSuccess = false,
                Message = "Error"
            };
            _mockProductService.Setup(s => s.GetAllProductAsync()).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error", badRequest.Value);
        }
        #endregion

        #region Detail Tests
        [Fact]
        public async Task Detail_ReturnsViewWithProduct_WhenIdExists()
        {
            // Arrange
            int id = 1;
            var product = new Product { Id = id, Name = "Laptop" };
            var serviceResult = new ServiceResult<Product> { IsSuccess = true, Data = product };

            _mockProductService.Setup(s => s.GetProductByIdAsync(id)).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Detail(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            // Kiểm tra xem Model có phải ProductViewModel không (giả định ToDetailVM trả về type này)
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task Detail_ReturnsErrorView_WhenProductNotFound()
        {
            // Arrange
            int id = 99;
            var serviceResult = new ServiceResult<Product> { IsSuccess = false };
            _mockProductService.Setup(s => s.GetProductByIdAsync(id)).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Detail(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
        }
        #endregion

        #region ByCategory Tests
        [Fact]
        public async Task ByCategory_SetsViewBagAndReturnsProducts()
        {
            // Arrange
            int catId = 1;
            var category = new Category { Id = catId, Name = "Electronics", Description = "Desc" };
            var products = new List<Product> { new Product { Id = 10, Name = "Phone" } };

            _mockCategoryService.Setup(s => s.GetByIdAsync(catId))
                .ReturnsAsync(new ServiceResult<Category> { IsSuccess = true, Data = category });

            _mockProductService.Setup(s => s.GetByCategory(catId))
                .ReturnsAsync(new ServiceResult<List<Product>> { IsSuccess = true, Data = products });

            // Act
            var result = await _controller.ByCategory(catId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Electronics", _controller.ViewBag.CategoryName);
            Assert.Equal("Desc", _controller.ViewBag.CategoryDescription);

            var model = Assert.IsType<List<ProductViewModel>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task ByCategory_ReturnsEmptyList_WhenNoProductsFound()
        {
            // Arrange
            int catId = 1;
            _mockCategoryService.Setup(s => s.GetByIdAsync(catId))
                .ReturnsAsync(new ServiceResult<Category> { IsSuccess = true });

            _mockProductService.Setup(s => s.GetByCategory(catId))
                .ReturnsAsync(new ServiceResult<List<Product>> { IsSuccess = true, Data = new List<Product>() });

            // Act
            var result = await _controller.ByCategory(catId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<ProductViewModel>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task ByCategory_ReturnsMessage_WhenServiceFails()
        {
            // Arrange
            int catId = 1;
            _mockCategoryService.Setup(s => s.GetByIdAsync(catId))
                .ReturnsAsync(new ServiceResult<Category> { IsSuccess = true });

            _mockProductService.Setup(s => s.GetByCategory(catId))
                .ReturnsAsync(new ServiceResult<List<Product>> { IsSuccess = false, Message = "Failed" });

            // Act
            var result = await _controller.ByCategory(catId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Failed", viewResult.ViewName);
            Assert.Null(viewResult.Model);
        }
        #endregion
    }
}
