using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using RetailSystem.API.Controllers;
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
    public class AuthControllerTest
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IAdminService> _adminServiceMock;

        private readonly AuthController _controller;

        public AuthControllerTest()
        {
            _configMock = new Mock<IConfiguration>();

            _adminServiceMock = new Mock<IAdminService>();

            _controller = new AuthController(
                _configMock.Object,
                _adminServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Login_WhenLoginFails_ShouldReturnBadRequest()
        {
            // Arrange
            var model = new LoginDTO
            {
                Username = "admin",
                Password = "123456"
            };

            _adminServiceMock
                .Setup(x => x.LoginAsync(model))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = false,
                    Message = "Invalid username or password"
                });

            // Act
            var result = await _controller.Login(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Invalid username or password",
                badRequest.Value
            );
        }

        [Fact]
        public async Task Login_WhenSuccess_ShouldReturnToken()
        {
            // Arrange
            var model = new LoginDTO
            {
                Username = "admin",
                Password = "123456"
            };

            _adminServiceMock
                .Setup(x => x.LoginAsync(model))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = true,
                    Data = "jwt-token"
                });

            // Act
            var result = await _controller.Login(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            okResult.Value.Should().NotBeNull();

            var value = okResult.Value;

            value.Should().BeEquivalentTo(new
            {
                token = "jwt-token"
            });
        }

        [Fact]
        public async Task Login_ShouldCallService()
        {
            // Arrange
            var model = new LoginDTO
            {
                Username = "admin",
                Password = "123456"
            };

            _adminServiceMock
                .Setup(x => x.LoginAsync(model))
                .ReturnsAsync(new ServiceResult<string>
                {
                    IsSuccess = true,
                    Data = "jwt-token"
                });

            // Act
            await _controller.Login(model);

            // Assert
            _adminServiceMock.Verify(
                x => x.LoginAsync(model),
                Times.Once
            );
        }

        [Fact]
        public async Task Register_WhenRegisterFails_ShouldReturnBadRequest()
        {
            // Arrange
            var model = new LoginDTO
            {
                Username = "admin",
                Password = "123456"
            };

            _adminServiceMock
                .Setup(x => x.RegisterAsync(model))
                .ReturnsAsync(new ServiceResult<AdminAccount>
                {
                    IsSuccess = false,
                    Message = "Username already exists"
                });

            // Act
            var result = await _controller.Register(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Username already exists",
                badRequest.Value
            );
        }

        [Fact]
        public async Task Register_WhenSuccess_ShouldReturnAdmin()
        {
            // Arrange
            var model = new LoginDTO
            {
                Username = "admin",
                Password = "123456"
            };

            var admin = new AdminAccount
            {
                Id = 1,
                Username = "admin",
                Role = "Admin"
            };

            _adminServiceMock
                .Setup(x => x.RegisterAsync(model))
                .ReturnsAsync(new ServiceResult<AdminAccount>
                {
                    IsSuccess = true,
                    Message = "Register success",
                    Data = admin
                });

            // Act
            var result = await _controller.Register(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            okResult.Value.Should().NotBeNull();

            okResult.Value.Should().BeEquivalentTo(new
            {
                message = "Register success",
                admin = admin
            });
        }

        [Fact]
        public async Task Register_ShouldCallService()
        {
            // Arrange
            var model = new LoginDTO
            {
                Username = "admin",
                Password = "123456"
            };

            _adminServiceMock
                .Setup(x => x.RegisterAsync(model))
                .ReturnsAsync(new ServiceResult<AdminAccount>
                {
                    IsSuccess = true,
                    Data = new AdminAccount()
                });

            // Act
            await _controller.Register(model);

            // Assert
            _adminServiceMock.Verify(
                x => x.RegisterAsync(model),
                Times.Once
            );
        }
    }
}
