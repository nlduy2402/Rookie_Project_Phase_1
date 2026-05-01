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

namespace RetailSystem.Tests.MvcController
{
    public class CartControllerTest
    {
        private readonly Mock<ICartService> _mockCartService;
        private readonly CartController _controller;
        private readonly string _userId = "user-123";

        public CartControllerTest()
        {
            _mockCartService = new Mock<ICartService>();
            _controller = new CartController(_mockCartService.Object);

            // User Claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, _userId),
            new Claim(ClaimTypes.Name, "test@example.com")
            }, "mock"));

            //assign User to ControllerContext
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewWithCart()
        {
            // Arrange
            var mockCart = new Cart(); 
            _mockCartService.Setup(s => s.GetCartAsync(_userId)).ReturnsAsync(mockCart);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(mockCart, viewResult.Model);
        }

        [Fact]
        public async Task Add_CallsServiceAndReturnsJson()
        {
            // Arrange
            int productId = 1;
            int qty = 2;
            var mockCartDto = new CartDTO();

            _mockCartService.Setup(s => s.GetCartDtoAsync(_userId)).ReturnsAsync(mockCartDto);

            // Act
            var result = await _controller.Add(productId, qty);

            // Assert
            _mockCartService.Verify(s => s.AddToCartAsync(_userId, productId, qty), Times.Once);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(mockCartDto, jsonResult.Value);
        }

        [Fact]
        public async Task UpdateQuantityAjax_RemovesItem_WhenQuantityIsZero()
        {
            // Arrange
            int productId = 99;
            int quantity = 0;

            _mockCartService.Setup(s => s.GetCartDtoAsync(_userId)).ReturnsAsync(new CartDTO());

            // Act
            var result = await _controller.UpdateQuantityAjax(productId, quantity);

            // Assert
            // quantity <= 0, service RemoveItemAsync is called instead of Update
            _mockCartService.Verify(s => s.RemoveItemAsync(_userId, productId), Times.Once);
            _mockCartService.Verify(s => s.UpdateQuantityAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Clear_RedirectsToIndex()
        {
            // Act
            var result = await _controller.Clear();

            // Assert
            _mockCartService.Verify(s => s.ClearCartAsync(_userId), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
