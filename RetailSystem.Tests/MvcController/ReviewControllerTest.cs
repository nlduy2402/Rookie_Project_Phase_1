using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using RetailSystem.CustomerSite.Controllers;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.MvcController
{
    public class ReviewControllerTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;

        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IReviewService> _reviewServiceMock;

        private readonly ReviewController _controller;

        public ReviewControllerTest()
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
            _reviewServiceMock = new Mock<IReviewService>();

            _controller = new ReviewController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _cartServiceMock.Object,
                _orderServiceMock.Object,
                _reviewServiceMock.Object
            );
        }

        [Fact]
        public async Task Rate_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Rate(0, 1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should().Be("Invalid input !");
        }

        [Fact]
        public async Task Rate_WhenProductIdInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Rate(1, 0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should().Be("Invalid input !");
        }

        [Fact]
        public async Task Rate_WhenUserNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns((string)null);

            // Act
            var result = await _controller.Rate(1, 1);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Rate_WhenOrderNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetOrderWithDetailsAsync(1, "u1"))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _controller.Rate(1, 1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Rate_WhenOrderNotCompleted_ShouldReturnBadRequest()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetOrderWithDetailsAsync(1, "u1"))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.Rate(1, 1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should()
                .Be("Order not eligible for review");
        }

        [Fact]
        public async Task Rate_WhenProductNotInOrder_ShouldReturnNotFound()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Completed,
                OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    ProductId = 99
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetOrderWithDetailsAsync(1, "u1"))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.Rate(1, 1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Rate_WhenValid_ShouldReturnView()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Completed,
                OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    ProductId = 1,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Images = new List<ProductImage>
                        {
                            new ProductImage
                            {
                                Url = "image.jpg"
                            }
                        }
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetOrderWithDetailsAsync(1, "u1"))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.Rate(1, 1);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Rate_ShouldReturnReviewViewModel()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Completed,
                OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    ProductId = 1,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Images = new List<ProductImage>
                        {
                            new ProductImage
                            {
                                Url = "image.jpg"
                            }
                        }
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetOrderWithDetailsAsync(1, "u1"))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.Rate(1, 1);

            // Assert
            var viewResult = result as ViewResult;

            viewResult.Model.Should()
                .BeOfType<ReviewViewModel>();
        }

        [Fact]
        public async Task Rate_ShouldMapReviewViewModelCorrectly()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Completed,
                OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    ProductId = 1,
                    Product = new Product
                    {
                        Name = "Laptop",
                        Images = new List<ProductImage>
                        {
                            new ProductImage
                            {
                                Url = "image.jpg"
                            }
                        }
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetOrderWithDetailsAsync(1, "u1"))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.Rate(1, 1);

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as ReviewViewModel;

            model.OrderId.Should().Be(1);

            model.ProductId.Should().Be(1);

            model.ProductName.Should().Be("Laptop");

            model.ImageUrl.Should().Be("image.jpg");
        }

        [Fact]
        public async Task Rate_ShouldCallGetOrderWithDetailsAsync()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Completed,
                OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    ProductId = 1,
                    Product = new Product
                    {
                        Images = new List<ProductImage>
                        {
                            new ProductImage()
                        }
                    }
                }
            }
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _orderServiceMock
                .Setup(x => x.GetOrderWithDetailsAsync(1, "u1"))
                .ReturnsAsync(order);

            // Act
            await _controller.Rate(1, 1);

            // Assert
            _orderServiceMock.Verify(
                x => x.GetOrderWithDetailsAsync(1, "u1"),
                Times.Once
            );
        }

        [Fact]
        public async Task RatePost_WhenAlreadyReviewed_ShouldRedirectToHistory()
        {
            // Arrange
            var vm = new ReviewViewModel
            {
                ProductId = 1,
                OrderId = 10,
                Rating = 5,
                Comment = "Good"
            };

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Rate(vm);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = result as RedirectToActionResult;

            redirect.ActionName.Should().Be("History");

            redirect.ControllerName.Should().Be("Order");
        }

        [Fact]
        public async Task RatePost_WhenAlreadyReviewed_ShouldSetTempDataError()
        {
            // Arrange
            var vm = new ReviewViewModel
            {
                ProductId = 1,
                OrderId = 10
            };

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(true);

            // Act
            await _controller.Rate(vm);

            // Assert
            _controller.TempData["ErrorMessage"]
                .Should()
                .Be("You have already reviewed this product!");
        }

        [Fact]
        public async Task RatePost_WhenNotReviewed_ShouldCreateReview()
        {
            // Arrange
            var vm = new ReviewViewModel
            {
                ProductId = 1,
                OrderId = 10,
                Rating = 5,
                Comment = "Excellent"
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(false);

            _reviewServiceMock
                .Setup(x => x.CreateReviewsAsync(
                    10,
                    "u1",
                    It.IsAny<List<ReviewItemDTO>>()
                ))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Rate(vm);

            // Assert
            _reviewServiceMock.Verify(
                x => x.CreateReviewsAsync(
                    10,
                    "u1",
                    It.IsAny<List<ReviewItemDTO>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task RatePost_WhenNotReviewed_ShouldRedirectToHistory()
        {
            // Arrange
            var vm = new ReviewViewModel
            {
                ProductId = 1,
                OrderId = 10,
                Rating = 4,
                Comment = "Nice"
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(false);

            _reviewServiceMock
                .Setup(x => x.CreateReviewsAsync(
                    10,
                    "u1",
                    It.IsAny<List<ReviewItemDTO>>()
                ))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Rate(vm);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = result as RedirectToActionResult;

            redirect.ActionName.Should().Be("History");

            redirect.ControllerName.Should().Be("Order");
        }

        [Fact]
        public async Task RatePost_ShouldCallIsReviewedAsync()
        {
            // Arrange
            var vm = new ReviewViewModel
            {
                ProductId = 1,
                OrderId = 10
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(true);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );
            // Act
            IActionResult result = await _controller.Rate(vm);
            // Assert
            _reviewServiceMock.Verify(
                x => x.IsReviewedAsync(1, 10, "u1"),
                Times.Once
            );
        }

        [Fact]
        public async Task RatePost_ShouldPassCorrectReviewData()
        {
            // Arrange
            var vm = new ReviewViewModel
            {
                ProductId = 5,
                OrderId = 20,
                Rating = 3,
                Comment = "Average"
            };

            List<ReviewItemDTO> capturedDto = null;

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(5, 20, "u1"))
                .ReturnsAsync(false);

            _reviewServiceMock
                .Setup(x => x.CreateReviewsAsync(
                    20,
                    "u1",
                    It.IsAny<List<ReviewItemDTO>>()
                ))
                .Callback<int, string, List<ReviewItemDTO>>(
                    (orderId, userId, dto) =>
                    {
                        capturedDto = dto;
                    })
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Rate(vm);

            // Assert
            capturedDto.Should().NotBeNull();

            capturedDto.Should().HaveCount(1);

            capturedDto.First().ProductId.Should().Be(5);

            capturedDto.First().Rating.Should().Be(3);

            capturedDto.First().Comment.Should().Be("Average");
        }

        [Fact]
        public async Task RatePost_ShouldUseCorrectUserId()
        {
            // Arrange
            var vm = new ReviewViewModel
            {
                ProductId = 1,
                OrderId = 10
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("custom-user");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "custom-user"))
                .ReturnsAsync(false);

            _reviewServiceMock
                .Setup(x => x.CreateReviewsAsync(
                    10,
                    "custom-user",
                    It.IsAny<List<ReviewItemDTO>>()
                ))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Rate(vm);

            // Assert
            _reviewServiceMock.Verify(
                x => x.CreateReviewsAsync(
                    10,
                    "custom-user",
                    It.IsAny<List<ReviewItemDTO>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task CheckReviewed_WhenOrderIdInvalid_ShouldReturnFalse()
        {
            // Act
            var result = await _controller.CheckReviewed(0, 1);

            // Assert
            result.Should().BeOfType<JsonResult>();

            var json = result as JsonResult;

            json.Value.Should().BeEquivalentTo(new
            {
                isReviewed = false
            });
        }

        [Fact]
        public async Task CheckReviewed_WhenProductIdInvalid_ShouldReturnFalse()
        {
            // Act
            var result = await _controller.CheckReviewed(1, 0);

            // Assert
            result.Should().BeOfType<JsonResult>();

            var json = result as JsonResult;

            json.Value.Should().BeEquivalentTo(new
            {
                isReviewed = false
            });
        }

        [Fact]
        public async Task CheckReviewed_WhenAlreadyReviewed_ShouldReturnTrue()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CheckReviewed(10, 1);

            // Assert
            result.Should().BeOfType<JsonResult>();

            var json = result as JsonResult;

            json.Value.Should().BeEquivalentTo(new
            {
                isReviewed = true
            });
        }

        [Fact]
        public async Task CheckReviewed_WhenNotReviewed_ShouldReturnFalse()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CheckReviewed(10, 1);

            // Assert
            result.Should().BeOfType<JsonResult>();

            var json = result as JsonResult;

            json.Value.Should().BeEquivalentTo(new
            {
                isReviewed = false
            });
        }

        [Fact]
        public async Task CheckReviewed_ShouldCallIsReviewedAsync()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("u1");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, "u1"))
                .ReturnsAsync(true);

            // Act
            await _controller.CheckReviewed(10, 1);

            // Assert
            _reviewServiceMock.Verify(
                x => x.IsReviewedAsync(1, 10, "u1"),
                Times.Once
            );
        }

        [Fact]
        public async Task CheckReviewed_ShouldUseCorrectParameters()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("custom-user");

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(5, 20, "custom-user"))
                .ReturnsAsync(true);

            // Act
            await _controller.CheckReviewed(20, 5);

            // Assert
            _reviewServiceMock.Verify(
                x => x.IsReviewedAsync(5, 20, "custom-user"),
                Times.Once
            );
        }

        [Fact]
        public async Task CheckReviewed_WhenUserIdIsNull_ShouldStillCallService()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _userManagerMock
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns((string)null);

            _reviewServiceMock
                .Setup(x => x.IsReviewedAsync(1, 10, null))
                .ReturnsAsync(false);

            // Act
            await _controller.CheckReviewed(10, 1);

            // Assert
            _reviewServiceMock.Verify(
                x => x.IsReviewedAsync(1, 10, null),
                Times.Once
            );
        }
    }
}
