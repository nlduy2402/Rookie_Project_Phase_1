using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.Service
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;

        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<ICartRepository> _cartRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;

        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;

        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();

            _orderRepoMock = new Mock<IOrderRepository>();
            _cartRepoMock = new Mock<ICartRepository>();
            _productRepoMock = new Mock<IProductRepository>();

            _cacheMock = new Mock<IMemoryCache>();
            _cacheEntryMock = new Mock<ICacheEntry>();

            // Setup repositories
            _uowMock
                .Setup(x => x.Orders)
                .Returns(_orderRepoMock.Object);

            _uowMock
                .Setup(x => x.Carts)
                .Returns(_cartRepoMock.Object);

            _uowMock.Setup(x => x.Products).Returns(_productRepoMock.Object);

            // Setup cache
            _cacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(_cacheEntryMock.Object);

            _orderService = new OrderService(
                _uowMock.Object,
                _cacheMock.Object
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WhenInputInvalid_ShouldThrowException()
        {
            // Arrange
            var dto = new OrderDTO();

            // Act
            Func<Task> act = async () =>
                await _orderService.CreateOrderAsync("", dto, "COD");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Invalid input");
        }

        [Fact]
        public async Task CreateOrderAsync_WhenCartIsNull_ShouldThrowException()
        {
            // Arrange
            var dto = new OrderDTO
            {
                FullName = "John"
            };

            _cartRepoMock
                .Setup(x => x.GetCartByUserIdAsync("u1"))
                .ReturnsAsync((Cart)null);

            // Act
            Func<Task> act = async () =>
                await _orderService.CreateOrderAsync("u1", dto, "COD");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Empty Cart");
        }

        [Fact]
        public async Task CreateOrderAsync_WhenCartHasNoItems_ShouldThrowException()
        {
            // Arrange
            var dto = new OrderDTO
            {
                FullName = "John"
            };

            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>()
            };

            _cartRepoMock
                .Setup(x => x.GetCartByUserIdAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            Func<Task> act = async () =>
                await _orderService.CreateOrderAsync("u1", dto, "COD");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Empty Cart");
        }

        [Fact]
        public async Task CreateOrderAsync_WhenStockNotEnough_ShouldRollbackAndThrow()
        {
            // Arrange
            var dto = new OrderDTO
            {
                FullName = "John",
                Address = "VN",
                PhoneNumber = "123"
            };

            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    Quantity = 5,
                    Product = new Product
                    {
                        Id = 1,
                        Name = "Laptop",
                        Quantity = 1,
                        Price = 1000
                    }
                }
            }
            };

            _cartRepoMock
                .Setup(x => x.GetCartByUserIdAsync("u1"))
                .ReturnsAsync(cart);

            // Act
            Func<Task> act = async () =>
                await _orderService.CreateOrderAsync("u1", dto, "COD");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Product Laptop is out of stock.");

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WithCOD_ShouldSetProcessingStatus()
        {
            // Arrange
            var dto = new OrderDTO
            {
                FullName = "John",
                Address = "VN",
                PhoneNumber = "123"
            };

            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    Quantity = 2,
                    Product = new Product
                    {
                        Id = 1,
                        Name = "Laptop",
                        Quantity = 10,
                        Price = 1000
                    }
                }
            }
            };

            _cartRepoMock
                .Setup(x => x.GetCartByUserIdAsync("u1"))
                .ReturnsAsync(cart);

            Order capturedOrder = null;

            _orderRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Order>()))
                .Callback<Order>(o => capturedOrder = o)
                .Returns(Task.CompletedTask);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _orderService.CreateOrderAsync("u1", dto, "COD");

            // Assert
            result.Should().NotBeNull();

            capturedOrder.Status
                .Should().Be(OrderStatus.Processing);

            capturedOrder.PaymentStatus
                .Should().Be(PaymentStatus.Pending);

            capturedOrder.TotalAmount
                .Should().Be(2000);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReduceProductQuantity()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Quantity = 10,
                Price = 1000
            };

            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    Quantity = 3,
                    Product = product
                }
            }
            };

            _cartRepoMock
                .Setup(x => x.GetCartByUserIdAsync("u1"))
                .ReturnsAsync(cart);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CreateOrderAsync(
                "u1",
                new OrderDTO(),
                "COD"
            );

            // Assert
            product.Quantity.Should().Be(7);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrderDetails()
        {
            // Arrange
            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    Quantity = 2,
                    Product = new Product
                    {
                        Id = 1,
                        Quantity = 10,
                        Price = 1500
                    }
                }
            }
            };

            _cartRepoMock
                .Setup(x => x.GetCartByUserIdAsync("u1"))
                .ReturnsAsync(cart);

            Order capturedOrder = null;

            _orderRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Order>()))
                .Callback<Order>(o => capturedOrder = o)
                .Returns(Task.CompletedTask);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CreateOrderAsync(
                "u1",
                new OrderDTO(),
                "COD"
            );

            // Assert
            capturedOrder.OrderDetails.Should().HaveCount(1);

            var detail = capturedOrder.OrderDetails.First();

            detail.ProductId.Should().Be(1);
            detail.Quantity.Should().Be(2);
            detail.Price.Should().Be(1500);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCommitTransaction()
        {
            // Arrange
            var cart = new Cart
            {
                UserId = "u1",
                Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    Quantity = 1,
                    Product = new Product
                    {
                        Id = 1,
                        Quantity = 10,
                        Price = 1000
                    }
                }
            }
            };

            _cartRepoMock
                .Setup(x => x.GetCartByUserIdAsync("u1"))
                .ReturnsAsync(cart);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CreateOrderAsync(
                "u1",
                new OrderDTO(),
                "COD"
            );

            // Assert
            _uowMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Once
            );

            _uowMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Once
            );

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task UpdatePaymentStatusAsync_WhenPaymentPaid_ShouldSetProcessingStatus()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                PaymentStatus = PaymentStatus.Pending,
                Status = OrderStatus.Pending
            };

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.UpdatePaymentStatusAsync(
                order,
                PaymentStatus.Paid
            );

            // Assert
            order.PaymentStatus.Should().Be(PaymentStatus.Paid);

            order.Status.Should().Be(OrderStatus.Processing);

            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdatePaymentStatusAsync_WhenPaymentNotPaid_ShouldNotChangeOrderStatus()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                PaymentStatus = PaymentStatus.Pending,
                Status = OrderStatus.Pending
            };

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.UpdatePaymentStatusAsync(
                order,
                PaymentStatus.Failed
            );

            // Assert
            order.PaymentStatus.Should().Be(PaymentStatus.Failed);

            // Status giữ nguyên
            order.Status.Should().Be(OrderStatus.Pending);

            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdatePaymentStatusAsync_ShouldSaveChanges()
        {
            // Arrange
            var order = new Order();

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.UpdatePaymentStatusAsync(
                order,
                PaymentStatus.Paid
            );

            // Assert
            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdatePaymentStatusAsync_ShouldUpdatePaymentStatus()
        {
            // Arrange
            var order = new Order
            {
                PaymentStatus = PaymentStatus.Pending
            };

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.UpdatePaymentStatusAsync(
                order,
                PaymentStatus.Paid
            );

            // Assert
            order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        }
        [Fact]
        public async Task CancelOrderAsync_WhenOrderNotFound_ShouldThrowException()
        {
            // Arrange
            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync((Order)null);

            // Act
            Func<Task> act = async () =>
                await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Order not found");
        }

        [Fact]
        public async Task CancelOrderAsync_WhenUserUnauthorized_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "other-user",
                PaymentMethod = "COD"
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Unauthorized");
        }

        [Fact]
        public async Task CancelOrderAsync_WhenPaymentMethodNotCOD_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "VNPAY"
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Only COD orders can be cancelled");
        }

        [Fact]
        public async Task CancelOrderAsync_WhenOrderAlreadyCompleted_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "COD",
                Status = OrderStatus.Completed
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Cannot cancel this order");
        }

        [Fact]
        public async Task CancelOrderAsync_WhenOrderAlreadyCancelled_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "COD",
                Status = OrderStatus.Cancelled
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Cannot cancel this order");
        }

        [Fact]
        public async Task CancelOrderAsync_WhenProductNotFound_ShouldRollbackAndThrow()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "COD",
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                ProductId = 1,
                Quantity = 2
            }
        }
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Product)null);

            // Act
            Func<Task> act = async () =>
                await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Product 1 not found");

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldRestoreProductQuantity()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Quantity = 5
            };

            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "COD",
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                ProductId = 1,
                Quantity = 3
            }
        }
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            product.Quantity.Should().Be(8);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldUpdateOrderStatus()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Quantity = 5
            };

            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "COD",
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                OrderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                ProductId = 1,
                Quantity = 2
            }
        }
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            order.Status.Should().Be(OrderStatus.Cancelled);

            order.PaymentStatus.Should().Be(PaymentStatus.Failed);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldCommitTransaction()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Quantity = 5
            };

            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "COD",
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                ProductId = 1,
                Quantity = 1
            }
        }
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            _uowMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Once
            );

            _uowMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Once
            );

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldSaveChanges()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Quantity = 5
            };

            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                PaymentMethod = "COD",
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                ProductId = 1,
                Quantity = 1
            }
        }
            };

            _orderRepoMock
                .Setup(x => x.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(product);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CancelOrderAsync(1, "u1");

            // Assert
            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task ShipOrderAsync_WhenOrderNotFound_ShouldThrowException()
        {
            // Arrange
            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Order)null);

            // Act
            Func<Task> act = async () =>
                await _orderService.ShipOrderAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Order not found");
        }

        [Fact]
        public async Task ShipOrderAsync_WhenOrderStatusInvalid_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Completed
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.ShipOrderAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Order not ready to ship");
        }

        [Fact]
        public async Task ShipOrderAsync_WhenStatusIsPending_ShouldShipSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Pending
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _orderService.ShipOrderAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Data.Should().Be("Order shipped successfully");

            order.Status.Should().Be(OrderStatus.Shipped);
        }

        [Fact]
        public async Task ShipOrderAsync_WhenStatusIsProcessing_ShouldShipSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Processing
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _orderService.ShipOrderAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.Shipped);
        }

        [Fact]
        public async Task ShipOrderAsync_ShouldBeginTransaction()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Pending
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.ShipOrderAsync(1);

            // Assert
            _uowMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task ShipOrderAsync_ShouldCommitTransaction()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Pending
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.ShipOrderAsync(1);

            // Assert
            _uowMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Once
            );

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task ShipOrderAsync_WhenSaveChangesFails_ShouldRollbackAndThrow()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Pending
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception());

            // Act
            Func<Task> act = async () =>
                await _orderService.ShipOrderAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Fail to ship this order.");

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task ShipOrderAsync_ShouldSaveChanges()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Pending
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.ShipOrderAsync(1);

            // Assert
            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task CompleteOrderAsync_WhenOrderNotFound_ShouldThrowException()
        {
            // Arrange
            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Order)null);

            // Act
            Func<Task> act = async () =>
                await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Order not found");
        }

        [Fact]
        public async Task CompleteOrderAsync_WhenUserUnauthorized_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "other-user",
                Status = OrderStatus.Shipped
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Unauthorized");
        }

        [Fact]
        public async Task CompleteOrderAsync_WhenOrderNotShipped_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                Status = OrderStatus.Processing
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Order is not ready to complete");
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldUpdateOrderStatus()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                Status = OrderStatus.Shipped,
                PaymentStatus = PaymentStatus.Pending
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            order.Status.Should().Be(OrderStatus.Completed);

            order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldBeginTransaction()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                Status = OrderStatus.Shipped
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            _uowMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldCommitTransaction()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                Status = OrderStatus.Shipped
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            _uowMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Once
            );

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Never
            );
        }

        [Fact]
        public async Task CompleteOrderAsync_WhenSaveChangesFails_ShouldRollbackTransaction()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                Status = OrderStatus.Shipped
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB Error"));

            // Act
            Func<Task> act = async () =>
                await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            await act.Should().ThrowAsync<Exception>();

            _uowMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldSaveChanges()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "u1",
                Status = OrderStatus.Shipped
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(order);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _orderService.CompleteOrderAsync(1, "u1");

            // Assert
            _uowMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once
            );
        }
    }
}
