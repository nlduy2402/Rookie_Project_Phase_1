using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RetailSystem.API.Controllers;
using RetailSystem.API.Shared;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.ApiController
{
    public class ProductControllerTest
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;

        private readonly ProductsController _controller;

        public ProductControllerTest()
        {
            _productServiceMock = new Mock<IProductService>();

            _loggerMock = new Mock<ILogger<ProductsController>>();

            _controller = new ProductsController(
                _productServiceMock.Object,
                _loggerMock.Object
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task GetAll_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetAllProductAsync())
                .ReturnsAsync(new ServiceResult<List<Product>>
                {
                    IsSuccess = false,
                    Message = "Error"
                });

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should().Be("Error");
        }

        [Fact]
        public async Task GetAll_WhenSuccess_ShouldReturnApiResponse()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop"
                }
            };

            _productServiceMock
                .Setup(x => x.GetAllProductAsync())
                .ReturnsAsync(new ServiceResult<List<Product>>
                {
                    IsSuccess = true,
                    Data = products
                });

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;

            okResult.Should().NotBeNull();

            var response = okResult.Value as ApiResponse<List<Product>>;

            response.Should().NotBeNull();

            response.Success.Should().BeTrue();

            response.Message.Should().Be("Success");

            response.Data.Should().HaveCount(1);

            response.Data.First().Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task GetAll_ShouldCallService()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetAllProductAsync())
                .ReturnsAsync(new ServiceResult<List<Product>>
                {
                    IsSuccess = true,
                    Data = new List<Product>()
                });

            // Act
            await _controller.GetAll();

            // Assert
            _productServiceMock.Verify(
                x => x.GetAllProductAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task GetById_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = false,
                    Message = "Product not found"
                });

            // Act
            var result = await _controller.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should().Be("Product not found");
        }

        [Fact]
        public async Task GetById_WhenSuccess_ShouldReturnOk()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop"
            };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = product
                });

            // Act
            var result = await _controller.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnCorrectData()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop"
            };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = product
                });

            // Act
            var result = await _controller.GetByIdAsync(1);

            // Assert
            var okResult = result as OkObjectResult;

            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_ShouldCallService()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = new Product()
                });

            // Act
            await _controller.GetByIdAsync(1);

            // Assert
            _productServiceMock.Verify(
                x => x.GetProductByIdAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateAsync_WhenModelStateInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            var model = new CreateProductDTO();

            // Act
            var result = await _controller.CreateAsync(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            var model = new CreateProductDTO
            {
                Name = "Laptop"
            };

            _productServiceMock
                .Setup(x => x.CreateAsync(model))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = false,
                    Message = "Create failed"
                });

            // Act
            var result = await _controller.CreateAsync(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Create failed", badRequest.Value);
        }

        [Fact]
        public async Task CreateAsync_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop"
            };

            var model = new CreateProductDTO
            {
                Name = "Laptop"
            };

            _productServiceMock
                .Setup(x => x.CreateAsync(model))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Message = "Created successfully",
                    Data = product
                });

            // Act
            var result = await _controller.CreateAsync(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<Product>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Created successfully", response.Message);

            Assert.Equal("Laptop", response.Data.Name);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallService()
        {
            // Arrange
            var model = new CreateProductDTO();

            _productServiceMock
                .Setup(x => x.CreateAsync(model))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = new Product()
                });

            // Act
            await _controller.CreateAsync(model);

            // Assert
            _productServiceMock.Verify(
                x => x.CreateAsync(model),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateAsync_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            var model = new UpdateProductDTO
            {
                Id = 2
            };

            // Act
            var result = await _controller.UpdateAsync(1, model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WhenModelStateInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            var model = new UpdateProductDTO
            {
                Id = 1
            };

            // Act
            var result = await _controller.UpdateAsync(1, model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            var model = new UpdateProductDTO
            {
                Id = 1
            };

            _productServiceMock
                .Setup(x => x.UpdateAsync(model))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = false,
                    Message = "Update failed"
                });

            // Act
            var result = await _controller.UpdateAsync(1, model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Update failed", badRequest.Value);
        }

        [Fact]
        public async Task UpdateAsync_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Updated Laptop"
            };

            var model = new UpdateProductDTO
            {
                Id = 1
            };

            _productServiceMock
                .Setup(x => x.UpdateAsync(model))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Message = "Updated successfully",
                    Data = product
                });

            // Act
            var result = await _controller.UpdateAsync(1, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<Product>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Updated successfully", response.Message);

            Assert.Equal("Updated Laptop", response.Data.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallService()
        {
            // Arrange
            var model = new UpdateProductDTO
            {
                Id = 1
            };

            _productServiceMock
                .Setup(x => x.UpdateAsync(model))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = new Product()
                });

            // Act
            await _controller.UpdateAsync(1, model);

            // Assert
            _productServiceMock.Verify(
                x => x.UpdateAsync(model),
                Times.Once
            );
        }

        [Fact]
        public async Task Delete_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_WhenProductNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "Product not found"
                });

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            notFound.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(new ServiceResult<bool>
                {
                    IsSuccess = true,
                    Message = "Deleted successfully"
                });

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<int>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Deleted successfully", response.Message);

            Assert.Equal(1, response.Data);
        }

        [Fact]
        public async Task Delete_ShouldCallService()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(new ServiceResult<bool>
                {
                    IsSuccess = true
                });

            // Act
            await _controller.Delete(1);

            // Assert
            _productServiceMock.Verify(
                x => x.DeleteAsync(1),
                Times.Once
            );
        }
    }
}
