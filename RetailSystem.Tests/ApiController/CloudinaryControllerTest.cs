using CloudinaryDotNet.Actions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RetailSystem.API.Controllers;
using RetailSystem.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.ApiController
{
    public class CloudinaryControllerTest
    {
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;

        private readonly CloudinaryController _controller;

        public CloudinaryControllerTest()
        {
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _controller = new CloudinaryController(
                _cloudinaryServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task UploadPhoto_WhenUploadFails_ShouldReturnBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();

            var uploadResult = new ImageUploadResult
            {
                Error = new Error
                {
                    Message = "Upload failed"
                }
            };

            _cloudinaryServiceMock
                .Setup(x => x.AddPhotoAsync(fileMock.Object))
                .ReturnsAsync(uploadResult);

            // Act
            var result = await _controller.UploadPhoto(fileMock.Object);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Upload failed", badRequest.Value);
        }

        [Fact]
        public async Task UploadPhoto_WhenSuccess_ShouldReturnPhotoInfo()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();

            var uploadResult = new ImageUploadResult
            {
                PublicId = "abc123",
                SecureUrl = new Uri("https://example.com/photo.jpg")
            };

            _cloudinaryServiceMock
                .Setup(x => x.AddPhotoAsync(fileMock.Object))
                .ReturnsAsync(uploadResult);

            // Act
            var result = await _controller.UploadPhoto(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            okResult.Value.Should().NotBeNull();

            okResult.Value.Should().BeEquivalentTo(new
            {
                PublicId = "abc123",
                Url = "https://example.com/photo.jpg"
            });
        }

        [Fact]
        public async Task UploadPhoto_ShouldCallCloudinaryService()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();

            _cloudinaryServiceMock
                .Setup(x => x.AddPhotoAsync(fileMock.Object))
                .ReturnsAsync(new ImageUploadResult
                {
                    PublicId = "abc123",
                    SecureUrl = new Uri("https://example.com/photo.jpg")
                });

            // Act
            await _controller.UploadPhoto(fileMock.Object);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.AddPhotoAsync(fileMock.Object),
                Times.Once
            );
        }
    }
}
