using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using RetailSystem.CustomerSite.Controllers;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Models;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.ResponseModels;
using RetailSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.MvcController
{
    public class OrderControllerTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;

        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IVNPayService> _vnPayServiceMock;

        private readonly OrderController _controller;

        public OrderControllerTest()
        {
            // UserManager
            var userStoreMock = new Mock<IUserStore<User>>();

            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // SignInManager
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null,
                null,
                null,
                null
            );

            _cartServiceMock = new Mock<ICartService>();
            _orderServiceMock = new Mock<IOrderService>();
            _vnPayServiceMock = new Mock<IVNPayService>();

            _controller = new OrderController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _cartServiceMock.Object,
                _orderServiceMock.Object,
                _vnPayServiceMock.Object
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1")
            }, "mock"));

            // Fake TempData
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Index_WhenCartIsNull_ShouldRedirectToCart()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync((Cart)null);

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = result as RedirectToActionResult;

            redirect.ActionName.Should().Be("Index");

            redirect.ControllerName.Should().Be("Cart");
        }

        [Fact]
        public async Task Index_WhenCartHasNoItems_ShouldRedirectToCart()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = result as RedirectToActionResult;

            redirect.ActionName.Should().Be("Index");

            redirect.ControllerName.Should().Be("Cart");
        }

        [Fact]
        public async Task Index_WhenStockNotEnough_ShouldRedirectToCart()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>
            {
                new CartItem
                {
                    Quantity = 10,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Quantity = 2
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = result as RedirectToActionResult;

            redirect.ActionName.Should().Be("Index");

            redirect.ControllerName.Should().Be("Cart");
        }

        [Fact]
        public async Task Index_WhenStockNotEnough_ShouldSetTempData()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>
            {
                new CartItem
                {
                    Quantity = 5,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Quantity = 1
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            await _controller.Index();

            // Assert
            _controller.TempData.ContainsKey("StockErrors")
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task Index_WhenCartValid_ShouldReturnView()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>
            {
                new CartItem
                {
                    Quantity = 1,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Quantity = 10
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Index_WhenCartValid_ShouldReturnCheckoutViewModel()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>
            {
                new CartItem
                {
                    Quantity = 1,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Quantity = 10
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;

            viewResult.Model.Should()
                .BeOfType<CheckoutViewModel>();
        }

        [Fact]
        public async Task Index_ShouldSetCartItemsToViewModel()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>
            {
                new CartItem
                {
                    Quantity = 2,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Quantity = 10
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as CheckoutViewModel;

            model.CartItems.Should().HaveCount(1);

            model.CartItems.First().Quantity.Should().Be(2);
        }

        [Fact]
        public async Task Index_ShouldInitializeOrderData()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>
            {
                new CartItem
                {
                    Quantity = 1,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Quantity = 10
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as CheckoutViewModel;

            model.OrderData.Should().NotBeNull();
        }

        [Fact]
        public async Task Index_ShouldCallGetCartAsync()
        {
            // Arrange
            var cart = new Cart
            {
                Items = new List<CartItem>
            {
                new CartItem
                {
                    Quantity = 1,
                    Product = new Product
                    {
                        Quantity = 10
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            await _controller.Index();

            // Assert
            _cartServiceMock.Verify(
                x => x.GetCartAsync("u1"),
                Times.Once
            );
        }

        [Fact]
        public async Task Create_WhenModelStateInvalid_ShouldReturnIndexView()
        {
            // Arrange
            _controller.ModelState.AddModelError(
                "FullName",
                "Required"
            );

            var model = new CheckoutViewModel();

            var cart = new Cart
            {
                Items = new List<CartItem>
        {
            new CartItem()
        }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Create(model);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().Be("Index");

            var vm = viewResult.Model as CheckoutViewModel;

            vm.CartItems.Should().HaveCount(1);
        }

        [Fact]
        public async Task Create_WhenPaymentMethodIsCOD_ShouldRedirectToHistory()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "COD",
                OrderData = new OrderDTO()
            };

            var order = new Order
            {
                Id = 1
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "COD"
                ))
                .ReturnsAsync(order);

            _cartServiceMock
                .Setup(x => x.ClearCartAsync("u1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = result as RedirectToActionResult;

            redirect.ActionName.Should().Be("History");

            redirect.ControllerName.Should().Be("Order");
        }

        [Fact]
        public async Task Create_WhenPaymentMethodIsCOD_ShouldClearCart()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "COD",
                OrderData = new OrderDTO()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "COD"
                ))
                .ReturnsAsync(new Order());

            _cartServiceMock
                .Setup(x => x.ClearCartAsync("u1"))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Create(model);

            // Assert
            _cartServiceMock.Verify(
                x => x.ClearCartAsync("u1"),
                Times.Once
            );
        }

        [Fact]
        public async Task Create_WhenPaymentMethodIsCOD_ShouldSetSuccessMessage()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "COD",
                OrderData = new OrderDTO()
            };

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "COD"
                ))
                .ReturnsAsync(new Order());

            _cartServiceMock
                .Setup(x => x.ClearCartAsync("u1"))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Create(model);

            // Assert
            _controller.TempData["SuccessMessage"]
                .Should()
                .Be("Order Success!");
        }

        [Fact]
        public async Task Create_WhenPaymentMethodIsVNPay_ShouldRedirectToVNPayUrl()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "VNPay",
                OrderData = new OrderDTO()
            };

            var order = new Order
            {
                Id = 1
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "VNPay"
                ))
                .ReturnsAsync(order);

            _vnPayServiceMock
                .Setup(x => x.CreatePaymentUrl(
                    It.IsAny<HttpContext>(),
                    order
                ))
                .Returns("https://sandbox.vnpayment.vn/payment");

            // Act
            var result = await _controller.Create(model);

            // Assert
            result.Should().BeOfType<RedirectResult>();

            var redirect = result as RedirectResult;

            redirect.Url.Should()
                .Be("https://sandbox.vnpayment.vn/payment");
        }

        [Fact]
        public async Task Create_WhenPaymentMethodUnknown_ShouldRedirectHome()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "OTHER",
                OrderData = new OrderDTO()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "OTHER"
                ))
                .ReturnsAsync(new Order());

            // Act
            var result = await _controller.Create(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = result as RedirectToActionResult;

            redirect.ActionName.Should().Be("Index");

            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public async Task Create_WhenExceptionOccurs_ShouldReturnIndexView()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "COD",
                OrderData = new OrderDTO()
            };

            var cart = new Cart
            {
                Items = new List<CartItem>
        {
            new CartItem()
        }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "COD"
                ))
                .ThrowsAsync(new Exception("Order failed"));

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Create(model);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().Be("Index");
        }

        [Fact]
        public async Task Create_WhenExceptionOccurs_ShouldAddModelError()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "COD",
                OrderData = new OrderDTO()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "COD"
                ))
                .ThrowsAsync(new Exception("Order failed"));

            _cartServiceMock
                .Setup(x => x.GetCartAsync("u1"))
                .ReturnsAsync(new Cart());

            // Act
            await _controller.Create(model);

            // Assert
            _controller.ModelState.IsValid.Should().BeFalse();

            _controller.ModelState[string.Empty]
                .Errors
                .First()
                .ErrorMessage
                .Should()
                .Contain("Order failed");
        }

        [Fact]
        public async Task Create_ShouldCallCreateOrderAsync()
        {
            // Arrange
            var model = new CheckoutViewModel
            {
                PaymentMethod = "COD",
                OrderData = new OrderDTO()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "COD"
                ))
                .ReturnsAsync(new Order());

            _cartServiceMock
                .Setup(x => x.ClearCartAsync("u1"))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Create(model);

            // Assert
            _orderServiceMock.Verify(
                x => x.CreateOrderAsync(
                    "u1",
                    model.OrderData,
                    "COD"
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task Create_WhenModelInvalid_AndCartNull_ShouldSetEmptyCartItems()
        {
            // Arrange
            _controller.ModelState.AddModelError(
                "Name",
                "Required"
            );

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("user-1");

            _cartServiceMock
                .Setup(x => x.GetCartAsync("user-1"))
                .ReturnsAsync((Cart)null);

            var model = new CheckoutViewModel
            {
                OrderData = new OrderDTO()
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Index", viewResult.ViewName);

            var vm =
                Assert.IsType<CheckoutViewModel>(viewResult.Model);

            Assert.NotNull(vm.CartItems);

            Assert.Empty(vm.CartItems);
        }

        [Fact]
        public async Task History_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 1, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = false,
                    Message = "Error"
                });

            // Act
            var result = await _controller.History();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should().Be("Error");
        }

        [Fact]
        public async Task History_WhenSuccess_ShouldReturnView()
        {
            // Arrange
            var orders = new List<Order>
    {
        new Order
        {
            Id = 1,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100,
            Status = OrderStatus.Pending,
            PaymentMethod = "COD",
            PaymentStatus = PaymentStatus.Pending,
            OrderDetails = new List<OrderDetail>()
        }
    };

            var pageResult = new PageResult<Order>
            {
                Items = orders,
                Page = 1,
                PageSize = 6,
                TotalCount = 1
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 1, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            var result = await _controller.History();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task History_ShouldReturnOrderHistoryViewModel()
        {
            // Arrange
            var orders = new List<Order>
    {
        new Order
        {
            Id = 1,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100,
            Status = OrderStatus.Pending,
            PaymentMethod = "COD",
            PaymentStatus = PaymentStatus.Pending,
            OrderDetails = new List<OrderDetail>()
        }
    };

            var pageResult = new PageResult<Order>
            {
                Items = orders,
                Page = 1,
                PageSize = 6,
                TotalCount = 1
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 1, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            var result = await _controller.History();

            // Assert
            var viewResult = result as ViewResult;

            viewResult.Model.Should()
                .BeOfType<OrderHistoryViewModel>();
        }

        [Fact]
        public async Task History_ShouldMapPaginationCorrectly()
        {
            // Arrange
            var pageResult = new PageResult<Order>
            {
                Items = new List<Order>(),
                Page = 2,
                PageSize = 6,
                TotalCount = 30
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 2, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            var result = await _controller.History(2);

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as OrderHistoryViewModel;

            model.Page.Should().Be(2);

            model.TotalPages.Should().Be(5);
        }

        [Fact]
        public async Task History_ShouldMapOrderItemsCorrectly()
        {
            // Arrange
            var orders = new List<Order>
    {
        new Order
        {
            Id = 10,
            TotalAmount = 500,
            PaymentMethod = "COD",
            Status = OrderStatus.Processing,
            PaymentStatus = PaymentStatus.Pending,
            OrderDetails = new List<OrderDetail>()
        }
    };

            var pageResult = new PageResult<Order>
            {
                Items = orders,
                Page = 1,
                PageSize = 6,
                TotalCount = 1
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 1, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            var result = await _controller.History();

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as OrderHistoryViewModel;

            model.Items.Should().HaveCount(1);

            model.Items.First().Id.Should().Be(10);

            model.Items.First().TotalAmount.Should().Be(500);

            model.Items.First().PaymentMethod.Should().Be("COD");
        }

        [Fact]
        public async Task History_ShouldMapOrderDetailsCorrectly()
        {
            // Arrange
            var orders = new List<Order>
    {
        new Order
        {
            Id = 1,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    ProductId = 5,
                    Quantity = 2,
                    Product = new Product
                    {
                        Name = "Laptop"
                    }
                }
            }
        }
    };

            var pageResult = new PageResult<Order>
            {
                Items = orders,
                Page = 1,
                PageSize = 6,
                TotalCount = 1
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 1, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            var result = await _controller.History();

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as OrderHistoryViewModel;

            model.Items.First()
                .OrderDetails
                .Should()
                .HaveCount(1);

            var detail = model.Items.First().OrderDetails.First();

            detail.ProductId.Should().Be(5);

            detail.ProductName.Should().Be("Laptop");

            detail.Quantity.Should().Be(2);
        }

        [Fact]
        public async Task History_ShouldCallGetUserOrdersPagedAsync()
        {
            // Arrange
            var pageResult = new PageResult<Order>
            {
                Items = new List<Order>(),
                Page = 1,
                PageSize = 6,
                TotalCount = 0
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 1, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            await _controller.History();

            // Assert
            _orderServiceMock.Verify(
                x => x.GetUserOrdersPagedAsync("u1", 1, 6),
                Times.Once
            );
        }

        [Fact]
        public async Task History_WhenNoOrders_ShouldReturnEmptyItems()
        {
            // Arrange
            var pageResult = new PageResult<Order>
            {
                Items = new List<Order>(),
                Page = 1,
                PageSize = 6,
                TotalCount = 0
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetUserOrdersPagedAsync("u1", 1, 6))
                .ReturnsAsync(new ServiceResult<PageResult<Order>>
                {
                    IsSuccess = true,
                    Data = pageResult
                });

            // Act
            var result = await _controller.History();

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as OrderHistoryViewModel;

            model.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task PaymentReturn_WhenResultIsNull_ShouldReturnFailView()
        {
            // Arrange
            _vnPayServiceMock
                .Setup(x => x.Execute(It.IsAny<IQueryCollection>()))
                .Returns((PaymentResult)null);

            // Act
            var result = await _controller.PaymentReturn();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Fail", viewResult.ViewName);
        }

        [Fact]
        public async Task PaymentReturn_WhenOrderNotFound_ShouldReturnFailView()
        {
            // Arrange
            var paymentResult = new PaymentResult
            {
                Success = true,
                TxnRef = "txn123"
            };

            _vnPayServiceMock
                .Setup(x => x.Execute(It.IsAny<IQueryCollection>()))
                .Returns(paymentResult);

            _orderServiceMock
                .Setup(x => x.GetByTxnRefAsync("txn123"))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _controller.PaymentReturn();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Fail", viewResult.ViewName);
        }

        [Fact]
        public async Task PaymentReturn_WhenPaymentSuccess_ShouldUpdateStatusAndClearCart()
        {
            // Arrange
            var paymentResult = new PaymentResult
            {
                Success = true,
                TxnRef = "txn123"
            };

            var order = new Order
            {
                Id = 1,
                UserId = "user-1"
            };

            _vnPayServiceMock
                .Setup(x => x.Execute(It.IsAny<IQueryCollection>()))
                .Returns(paymentResult);

            _orderServiceMock
                .Setup(x => x.GetByTxnRefAsync("txn123"))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.PaymentReturn();

            // Assert
            _orderServiceMock.Verify(
                x => x.UpdatePaymentStatusAsync(
                    order,
                    PaymentStatus.Paid),
                Times.Once
            );

            _cartServiceMock.Verify(
                x => x.ClearCartAsync("user-1"),
                Times.Once
            );

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Success", viewResult.ViewName);
        }

        [Fact]
        public async Task PaymentReturn_WhenPaymentFailed_ShouldUpdateStatusFailed()
        {
            // Arrange
            var paymentResult = new PaymentResult
            {
                Success = false,
                TxnRef = "txn123"
            };

            var order = new Order
            {
                Id = 1,
                UserId = "user-1"
            };

            _vnPayServiceMock
                .Setup(x => x.Execute(It.IsAny<IQueryCollection>()))
                .Returns(paymentResult);

            _orderServiceMock
                .Setup(x => x.GetByTxnRefAsync("txn123"))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.PaymentReturn();

            // Assert
            _orderServiceMock.Verify(
                x => x.UpdatePaymentStatusAsync(
                    order,
                    PaymentStatus.Failed),
                Times.Once
            );

            _cartServiceMock.Verify(
                x => x.ClearCartAsync(It.IsAny<string>()),
                Times.Never
            );

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Fail", viewResult.ViewName);
        }

        [Fact]
        public async Task Cancel_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Cancel(0);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Invalid input !", badRequest.Value);
        }

        [Fact]
        public async Task Cancel_WhenSuccess_ShouldRedirectHistory()
        {
            // Act
            var result = await _controller.Cancel(1);

            // Assert
            _orderServiceMock.Verify(
                x => x.CancelOrderAsync(1, It.IsAny<string>()),
                Times.Once
            );

            Assert.Equal(
                "Order cancelled successfully!",
                _controller.TempData["SuccessMessage"]
            );

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("History", redirect.ActionName);
        }

        [Fact]
        public async Task Cancel_WhenExceptionOccurs_ShouldSetErrorMessage()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.CancelOrderAsync(
                    1,
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Cancel failed"));

            // Act
            var result = await _controller.Cancel(1);

            // Assert
            Assert.Equal(
                "Cancel failed",
                _controller.TempData["ErrorMessage"]
            );

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("History", redirect.ActionName);
        }

        [Fact]
        public async Task Received_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Received(0);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Invalid input !", badRequest.Value);
        }

        [Fact]
        public async Task Received_WhenSuccess_ShouldRedirectHistory()
        {
            // Act
            var result = await _controller.Received(1);

            // Assert
            _orderServiceMock.Verify(
                x => x.CompleteOrderAsync(1,
                    It.IsAny<string>()),
                Times.Once
            );

            Assert.Equal(
                "Order completed!",
                _controller.TempData["SuccessMessage"]
            );

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("History", redirect.ActionName);
        }

        [Fact]
        public async Task Received_WhenExceptionOccurs_ShouldSetErrorMessage()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.CompleteOrderAsync(
                    1,
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Complete failed"));

            // Act
            var result = await _controller.Received(1);

            // Assert
            Assert.Equal(
                "Complete failed",
                _controller.TempData["ErrorMessage"]
            );

            var redirect =
                Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("History", redirect.ActionName);
        }
    }
}
