using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _userServiceMock;

        private readonly UsersController _controller;

        public UserControllerTest()
        {
            _userServiceMock = new Mock<IUserService>();

            _controller = new UsersController(
                _userServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task GetAll_WhenServiceFails_ShouldReturnNotFound()
        {
            // Arrange
            _userServiceMock
                .Setup(x => x.GetAllCustomerAccountsAsync())
                .ReturnsAsync(new ServiceResult<List<UserDTO>>
                {
                    IsSuccess = false,
                    Message = "No users found"
                });

            // Act
            var result = await _controller.GetAll();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("No users found", notFound.Value);
        }

        [Fact]
        public async Task GetAll_WhenSuccess_ShouldReturnOkResponse()
        {
            // Arrange
            var users = new List<UserDTO>
            {
                new UserDTO
                {
                    Id = "1",
                    UserName = "customer1",
                    Email = "customer1@gmail.com"
                }
            };

            _userServiceMock
                .Setup(x => x.GetAllCustomerAccountsAsync())
                .ReturnsAsync(new ServiceResult<List<UserDTO>>
                {
                    IsSuccess = true,
                    Data = users
                });

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ApiResponse<List<UserDTO>>>(okResult.Value);

            Assert.True(response.Success);

            Assert.Single(response.Data);

            Assert.Equal("customer1", response.Data.First().UserName);
        }

        [Fact]
        public async Task GetAll_ShouldCallService()
        {
            // Arrange
            _userServiceMock
                .Setup(x => x.GetAllCustomerAccountsAsync())
                .ReturnsAsync(new ServiceResult<List<UserDTO>>
                {
                    IsSuccess = true,
                    Data = new List<UserDTO>()
                });

            // Act
            await _controller.GetAll();

            // Assert
            _userServiceMock.Verify(
                x => x.GetAllCustomerAccountsAsync(),
                Times.Once
            );
        }
    }
}
