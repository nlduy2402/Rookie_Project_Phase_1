using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.Service
{
    public class CartServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IBaseRepository<Cart>> _cartRepoMock;
        private readonly Mock<IBaseRepository<Product>> _productRepoMock;

        private readonly CartService _service;

        public CartServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _cartRepoMock = new Mock<IBaseRepository<Cart>>();
            _productRepoMock = new Mock<IBaseRepository<Product>>();

            //_uowMock.Setup(x => x.Carts).Returns(_cartRepoMock.Object);
            _uowMock.Setup(x => x.Products).Returns(_productRepoMock.Object);

            var cache = new MemoryCache(new MemoryCacheOptions());

            _service = new CartService(_uowMock.Object, cache);
        }

        // Test GetCartAsync when cart exists, it should return the cart
        [Fact]
        public async Task GetCart_Should_Return_Cart()
        {
            var cart = new Cart { UserId = "u1" };

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Cart, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(cart);

            var result = await _service.GetCartAsync("u1");

            result.Should().NotBeNull();
        }

        // Test GetCartAsync when cart not exists, it should return null
        [Fact]
        public async Task GetCart_Should_Return_Null()
        {
            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Cart, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync((Cart)null);

            var result = await _service.GetCartAsync("u1");

            result.Should().BeNull();
        }

        // Test AddToCartAsync when product not exists, it should throw "Product not found"
        [Fact]
        public async Task AddToCart_Should_Throw_When_Quantity_Invalid()
        {
            await Assert.ThrowsAsync<Exception>(() =>
                _service.AddToCartAsync("u1", 1, 0));
        }
        // Test AddToCartAsync when product not exists, it should throw "Product not found"
        [Fact]
        public async Task AddToCart_Should_Throw_When_Product_Not_Exist()
        {
            _productRepoMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.AddToCartAsync("u1", 1, 1));
        }

        // Test AddToCartAsync when cart not exists, it should create new cart
        [Fact]
        public async Task AddToCart_Should_Create_New_Cart()
        {
            _productRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Product { Id = 1 });

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "Items"))
                .ReturnsAsync((Cart)null);

            _cartRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Cart>()))
                .Returns(Task.CompletedTask);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            await _service.AddToCartAsync("u1", 1, 2);

            _cartRepoMock.Verify(x => x.CreateAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task AddToCart_Should_Add_New_Item()
        {
            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>()
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Product { Id = 1 });

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "Items"))
                .ReturnsAsync(cart);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            await _service.AddToCartAsync("u1", 1, 2);

            cart.Items.Should().HaveCount(1);
        }

        // Test AddToCartAsync when item already in cart, it should increase quantity
        [Fact]
        public async Task AddToCart_Should_Increase_Quantity()
        {
            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>
        {
            new CartItem { ProductId = 1, Quantity = 1 }
        }
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Product { Id = 1 });

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "Items"))
                .ReturnsAsync(cart);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            await _service.AddToCartAsync("u1", 1, 2);

            cart.Items.First().Quantity.Should().Be(3);
        }

        // Test RemoveItemAsync when item exists, it should remove the item
        [Fact]
        public async Task RemoveItem_Should_Remove_Item()
        {
            var cart = new Cart
            {
                Items = new List<CartItem>
        {
            new CartItem { ProductId = 1 }
        }
            };

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "Items"))
                .ReturnsAsync(cart);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            await _service.RemoveItemAsync("u1", 1);

            cart.Items.Should().BeEmpty();
        }

        // Test RemoveItemAsync when item not exists, it should do nothing
        [Fact]
        public async Task RemoveItem_Should_Do_Nothing_When_Not_Exist()
        {
            var cart = new Cart { Items = new List<CartItem>() };

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "Items"))
                .ReturnsAsync(cart);

            await _service.RemoveItemAsync("u1", 1);

            cart.Items.Should().BeEmpty();
        }

        // Test UpdateQuantityAsync when item exists, it should update quantity
        [Fact]
        public async Task UpdateQuantity_Should_Update()
        {
            var cart = new Cart
            {
                Items = new List<CartItem>
        {
            new CartItem { ProductId = 1, Quantity = 1 }
        }
            };

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "Items"))
                .ReturnsAsync(cart);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            await _service.UpdateQuantityAsync("u1", 1, 5);

            cart.Items.First().Quantity.Should().Be(5);
        }

        // Test UpdateQuantityAsync when item not exists, it should do nothing
        [Fact]
        public async Task ClearCart_Should_Remove_All_Items()
        {
            var cart = new Cart
            {
                Items = new List<CartItem>
        {
            new CartItem(),
            new CartItem()
        }
            };

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), "Items"))
                .ReturnsAsync(cart);

            _uowMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            await _service.ClearCartAsync("u1");

            cart.Items.Should().BeEmpty();
        }

        // Test GetCartDtoAsync when cart exists, it should map to CartDTO correctly
        [Fact]
        public async Task GetCartDto_Should_Map_Correctly()
        {
            var cart = new Cart
            {
                Items = new List<CartItem>
        {
            new CartItem
            {
                ProductId = 1,
                Quantity = 2,
                Product = new Product
                {
                    Name = "P1",
                    Price = 100,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { Url = "img.jpg" }
                    }
                }
            }
        }
            };

            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(cart);

            var result = await _service.GetCartDtoAsync("u1");

            result.Count.Should().Be(2);
            result.Total.Should().Be(200);
            result.Items.First().Image.Should().Be("img.jpg");
        }

        // Test GetCartDtoAsync when cart not exists, it should return empty CartDTO
        [Fact]
        public async Task GetCartDto_Should_Return_Empty_When_No_Cart()
        {
            _cartRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Cart, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((Cart)null);

            var result = await _service.GetCartDtoAsync("u1");

            result.Items.Should().BeEmpty();
        }
    }
}
