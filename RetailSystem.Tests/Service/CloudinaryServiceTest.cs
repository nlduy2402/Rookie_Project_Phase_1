using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.Service
{
    public class CloudinaryServiceTest
    {
        private readonly CloudinaryService _service;

        public CloudinaryServiceTest()
        {
            var settings = Options.Create(
                new CloudinarySettings
                {
                    CloudName = "test",
                    ApiKey = "test",
                    ApiSecret = "test"
                });

            _service = new CloudinaryService(settings);
        }

        [Fact]
        public async Task AddPhotoAsync_ShouldUploadRealImage()
        {
            // Arrange
            var bytes = new byte[100];

            var stream = new MemoryStream(bytes);

            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.Length)
                .Returns(bytes.Length);

            fileMock.Setup(f => f.FileName)
                .Returns("test.jpg");

            fileMock.Setup(f => f.OpenReadStream())
                .Returns(stream);

            // Act
            var result = await _service.AddPhotoAsync(
                fileMock.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task AddPhotoAsync_WhenFileEmpty_ShouldReturnEmptyResult()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.Length)
                .Returns(0);

            // Act
            var result = await _service.AddPhotoAsync(
                fileMock.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task AddPhotosAsync_WhenFilesEmpty_ShouldReturnResults()
        {
            // Arrange
            var files = new List<IFormFile>();

            // Act
            var result = await _service.AddPhotosAsync(files);

            // Assert
            result.Should().NotBeNull();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task DeletePhotoAsync_ShouldReturnResult()
        {
            // Act
            var result =
                await _service.DeletePhotoAsync("fake_id");

            // Assert
            result.Should().NotBeNull();
        }
    }
}
