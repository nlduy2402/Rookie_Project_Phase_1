using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RetailSystem.CustomerSite.Controllers;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.ResponseModels;

namespace RetailSystem.Tests.MvcController
{
    public class CartControllerTest
    {

        private readonly Mock<ICartService> _cartServiceMock;

        private readonly CartController _controller;

        public CartControllerTest()
        {
            _cartServiceMock = new Mock<ICartService>();

            _controller = new CartController(
                _cartServiceMock.Object
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "user-1")
        }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithCart()
        {
            // Arrange
            var cart = new Cart
            {
                UserId = "user-1"
            };

            _cartServiceMock
                .Setup(x => x.GetCartAsync("user-1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(cart, viewResult.Model);
        }

        [Fact]
        public async Task GetCartJson_ShouldReturnJsonResult()
        {
            // Arrange
            var cartDto = new CartDTO();

            _cartServiceMock
                .Setup(x => x.GetCartDtoAsync("user-1"))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.GetCartJson();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            Assert.Equal(cartDto, jsonResult.Value);
        }

        [Fact]
        public async Task Add_ShouldCallAddToCartAsync()
        {
            // Arrange
            var cartDto = new CartDTO();

            _cartServiceMock
                .Setup(x => x.GetCartDtoAsync("user-1"))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.Add(1, 2);

            // Assert
            _cartServiceMock.Verify(
                x => x.AddToCartAsync("user-1", 1, 2),
                Times.Once
            );

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task AddAjax_ShouldCallAddToCartAsync()
        {
            // Arrange
            var cartDto = new CartDTO();

            _cartServiceMock
                .Setup(x => x.GetCartDtoAsync("user-1"))
                .ReturnsAsync(cartDto);

            _cartServiceMock
                .Setup(x => x.GetCartAsync("user-1"))
                .ReturnsAsync(new Cart());

            // Act
            var result = await _controller.AddAjax(1, 3);

            // Assert
            _cartServiceMock.Verify(
                x => x.AddToCartAsync("user-1", 1, 3),
                Times.Once
            );

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task RemoveAjax_ShouldCallRemoveItemAsync()
        {
            // Arrange
            var cartDto = new CartDTO();

            _cartServiceMock
                .Setup(x => x.GetCartDtoAsync("user-1"))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.RemoveAjax(1);

            // Assert
            _cartServiceMock.Verify(
                x => x.RemoveItemAsync("user-1", 1),
                Times.Once
            );

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task UpdateQuantityAjax_WhenQuantityLessThanOrEqualZero_ShouldRemoveItem()
        {
            // Arrange
            var cartDto = new CartDTO();

            _cartServiceMock
                .Setup(x => x.GetCartDtoAsync("user-1"))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.UpdateQuantityAjax(1, 0);

            // Assert
            _cartServiceMock.Verify(
                x => x.RemoveItemAsync("user-1", 1),
                Times.Once
            );

            _cartServiceMock.Verify(
                x => x.UpdateQuantityAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never
            );

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task UpdateQuantityAjax_WhenQuantityGreaterThanZero_ShouldUpdateQuantity()
        {
            // Arrange
            var cartDto = new CartDTO();

            _cartServiceMock
                .Setup(x => x.GetCartDtoAsync("user-1"))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.UpdateQuantityAjax(1, 5);

            // Assert
            _cartServiceMock.Verify(
                x => x.UpdateQuantityAsync("user-1", 1, 5),
                Times.Once
            );

            _cartServiceMock.Verify(
                x => x.RemoveItemAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never
            );

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task Remove_ShouldRedirectToIndex()
        {
            // Act
            var result = await _controller.Remove(1);

            // Assert
            _cartServiceMock.Verify(
                x => x.RemoveItemAsync("user-1", 1),
                Times.Once
            );

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Update_ShouldRedirectToIndex()
        {
            // Act
            var result = await _controller.Update(1, 4);

            // Assert
            _cartServiceMock.Verify(
                x => x.UpdateQuantityAsync("user-1", 1, 4),
                Times.Once
            );

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Clear_ShouldRedirectToIndex()
        {
            // Act
            var result = await _controller.Clear();

            // Assert
            _cartServiceMock.Verify(
                x => x.ClearCartAsync("user-1"),
                Times.Once
            );

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
