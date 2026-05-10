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
    public class CategoryControllerTest
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<ILogger<CategoriesController>> _loggerMock;

        private readonly CategoriesController _controller;

        public CategoryControllerTest()
        {
            _categoryServiceMock = new Mock<ICategoryService>();

            _loggerMock = new Mock<ILogger<CategoriesController>>();

            _controller = new CategoriesController(
                _categoryServiceMock.Object,
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
            _categoryServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new ServiceResult<List<Category>>
                {
                    IsSuccess = false,
                    Message = "Load failed"
                });

            // Act
            var result = await _controller.GetAll();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Load failed", badRequest.Value);
        }

        [Fact]
        public async Task GetAll_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var categories = new List<Category>
        {
            new Category
            {
                Id = 1,
                Name = "Laptop"
            }
        };

            _categoryServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new ServiceResult<List<Category>>
                {
                    IsSuccess = true,
                    Data = categories
                });

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<List<Category>>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Single(response.Data);
        }

        [Fact]
        public async Task GetById_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.GetById(0);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Invalid input !", badRequest.Value);
        }

        [Fact]
        public async Task GetById_WhenCategoryNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Category>
                {
                    IsSuccess = false,
                    Message = "Category not found"
                });

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("Category not found", notFound.Value);
        }

        [Fact]
        public async Task GetById_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Laptop"
            };

            _categoryServiceMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Category>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = category
                });

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<Category>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Laptop", response.Data.Name);
        }

        [Fact]
        public async Task Create_WhenModelNull_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Create(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Invalid input !", badRequest.Value);
        }

        [Fact]
        public async Task Create_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            var model = new CreateCategoryDTO
            {
                Name = "Laptop"
            };

            _categoryServiceMock
                .Setup(x => x.CreateAsync(model))
                .ReturnsAsync(new ServiceResult<Category>
                {
                    IsSuccess = false,
                    Message = "Create failed"
                });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Create failed", badRequest.Value);
        }

        [Fact]
        public async Task Create_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var model = new CreateCategoryDTO
            {
                Name = "Laptop"
            };

            var category = new Category
            {
                Id = 1,
                Name = "Laptop"
            };

            _categoryServiceMock
                .Setup(x => x.CreateAsync(model))
                .ReturnsAsync(new ServiceResult<Category>
                {
                    IsSuccess = true,
                    Message = "Created successfully",
                    Data = category
                });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<Category>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Laptop", response.Data.Name);
        }

        [Fact]
        public async Task Update_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            var model = new UpdateCategoryDTO();

            // Act
            var result = await _controller.Update(0, model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_WhenModelNull_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Update(1, null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_WhenCategoryNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var model = new UpdateCategoryDTO
            {
                Name = "Updated"
            };

            _categoryServiceMock
                .Setup(x => x.UpdateAsync(1, model))
                .ReturnsAsync(new ServiceResult<Category>
                {
                    IsSuccess = false,
                    Message = "Category not found"
                });

            // Act
            var result = await _controller.Update(1, model);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("Category not found", notFound.Value);
        }

        [Fact]
        public async Task Update_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var model = new UpdateCategoryDTO
            {
                Name = "Updated"
            };

            var category = new Category
            {
                Id = 1,
                Name = "Updated"
            };

            _categoryServiceMock
                .Setup(x => x.UpdateAsync(1, model))
                .ReturnsAsync(new ServiceResult<Category>
                {
                    IsSuccess = true,
                    Message = "Updated successfully",
                    Data = category
                });

            // Act
            var result = await _controller.Update(1, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<Category>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Updated", response.Data.Name);
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
        public async Task Delete_WhenCategoryNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = false,
                    Message = "Error Occured While Deleting Category"
                });

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(
                "Error Occured While Deleting Category",
                notFound.Value
            );
        }

        [Fact]
        public async Task Delete_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = true,
                    Data = "Category Deleted !"
                });

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<string>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Equal("Category Deleted !", response.Data);
        }

        [Fact]
        public async Task Delete_WhenServiceThrows_ShouldThrowException()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ThrowsAsync(new Exception("DB Error"));

            // Act
            Func<Task> act = async () => await _controller.Delete(1);

            // Assert
            var ex = await Assert.ThrowsAsync<Exception>(act);

            Assert.Equal("Can not do this !", ex.Message);
        }
    }
}
