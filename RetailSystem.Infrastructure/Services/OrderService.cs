using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Repository;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services.Base;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.ResponseModels;

namespace RetailSystem.Infrastructure.Services
{
    public class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IUnitOfWork _uow;
        private IMemoryCache _cache;
        public OrderService(IUnitOfWork uow, IMemoryCache cache) : base(uow, cache)
        {
            _uow = uow;
            _cache = cache;
        }
        protected override IBaseRepository<Order> GetRepository() => _uow.Orders;

        public async Task<Order> CreateOrderAsync(string userId, OrderDTO orderDto, string PaymentMethod)
        {
            var cart = await _uow.Carts.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any()) throw new Exception("Empty Cart");

            await _uow.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    FullName = orderDto.FullName,
                    Address = orderDto.Address,
                    PhoneNumber = orderDto.PhoneNumber,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Items.Sum(x => x.Quantity * x.Product.Price),
                    PaymentMethod = PaymentMethod,
                    PaymentStatus = PaymentStatus.Pending,
                    
                    TxnRef = Guid.NewGuid().ToString("N")
                };
                if(PaymentMethod == "COD")
                {
                    order.Status = OrderStatus.Processing;
                }

                // 3. Chuyển CartItem sang OrderDetail và trừ kho
                foreach (var item in cart.Items)
                {
                    // Kiểm tra tồn kho
                    if (item.Product.Quantity < item.Quantity)
                        throw new Exception($"Product {item.Product.Name} is out of stock.");

                    // Thêm detail trực tiếp vào list của Order
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price // Lưu giá tĩnh
                    });

                    // Cập nhật số lượng sản phẩm
                    item.Product.Quantity -= item.Quantity;
                }

                // 4. Lưu Order (EF sẽ tự lưu luôn các OrderDetail vì chúng nằm trong List của Order)
                await _uow.Orders.CreateAsync(order);

                // 5. Xóa giỏ hàng
                //_uow.Carts.ClearCart(cart);


                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return order;
            }
            catch (Exception)
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        //public async Task<IEnumerable<Order>> GetOrderHistoryAsync(string userId)
        //{

        //    return await _uow.Orders.GetOrderHistoryByUserIdAsync(userId);
        //}
        public async Task<Order?> GetByTxnRefAsync(string txnRef)
        {
            return await _uow.Orders.GetFirstOrDefaultAsync(x => x.TxnRef == txnRef);
        }
        public async Task UpdatePaymentStatusAsync(Order order, PaymentStatus paymentStatus)
        {
            order.PaymentStatus = paymentStatus;

            if (paymentStatus == PaymentStatus.Paid)
            {
                order.Status = OrderStatus.Processing;
            }

            await _uow.SaveChangesAsync();
        }
        public async Task CancelOrderAsync(int orderId, string userId)
        {
            var order = await _uow.Orders.GetOrderWithDetailsAsync(orderId);


            if (order == null)
                throw new Exception("Order not found");

            if (order.UserId != userId)
                throw new Exception("Unauthorized");

            if (order.PaymentMethod != "COD")
                throw new Exception("Only COD orders can be cancelled");

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                throw new Exception("Cannot cancel this order");

            await _uow.BeginTransactionAsync();
            try
            {
                foreach (var item in order.OrderDetails)
                {
                    var product = await _uow.Products.GetByIdAsync(item.ProductId);

                    if (product == null)
                        throw new Exception($"Product {item.ProductId} not found");

                    product.Quantity += item.Quantity;
                }

                // 3. Update order
                order.Status = OrderStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Failed;

                // 4. Save
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<PageResult<Order>>> GetUserOrdersPagedAsync(
            string userId, int page, int pageSize)
        {
            var result = await _uow.Orders.GetOrderHistoryByUserIdAsync(userId, page, pageSize);

            return new ServiceResult<PageResult<Order>>
            {
                IsSuccess = true,
                Data = result

            };
        }
        public async Task<Order> GetOrderWithDetailsAsync(int orderId, string userId)
        {
            var order = await _uow.Orders.GetOrderWithDetailsAsync(orderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.UserId != userId)
                throw new Exception("Unauthorized");

            return order;
        }
        //public async Task ShipOrderAsync(int orderId, string userId)
        //{
        //    var order = await _uow.Orders.GetByIdAsync(orderId);

        //    if (order == null)
        //        throw new Exception("Order not found");

        //    if (order.UserId != userId)
        //        throw new Exception("Unauthorized");

        //    if (order.Status != OrderStatus.Processing && order.Status != OrderStatus.Pending)
        //        throw new Exception("Order not ready to ship");

        //    await _uow.BeginTransactionAsync();

        //    try
        //    {
        //        order.Status = OrderStatus.Shipped;

        //        await _uow.SaveChangesAsync();
        //        await _uow.CommitTransactionAsync();
        //    }
        //    catch
        //    {
        //        await _uow.RollbackTransactionAsync();
        //        throw;
        //    }
        //}

        public async Task<ServiceResult<string>> ShipOrderAsync(int orderId)
        {
            var order = await _uow.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");
            if (order.Status != OrderStatus.Processing && order.Status != OrderStatus.Pending)
                throw new Exception("Order not ready to ship");
            await _uow.BeginTransactionAsync();
            try
            {
                order.Status = OrderStatus.Shipped;
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
                return new ServiceResult<string>
                {
                    IsSuccess = true,
                    Data = "Order shipped successfully"
                };
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw new Exception("Fail to ship this order.");
            }

        }

        public async Task CompleteOrderAsync(int orderId, string userId)
        {
            var order = await _uow.Orders.GetByIdAsync(orderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.UserId != userId)
                throw new Exception("Unauthorized");

            if (order.Status != OrderStatus.Shipped)
                throw new Exception("Order is not ready to complete");

            await _uow.BeginTransactionAsync();

            try
            {
                order.Status = OrderStatus.Completed;
                order.PaymentStatus = PaymentStatus.Paid;

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<PageResult<Order>>> GetAllOrdersPagedAsync(int page, int pageSize)
        {
            var result = await _uow.Orders.GetAllOrdersPagedAsync(page, pageSize);
            return new ServiceResult<PageResult<Order>>
            {
                IsSuccess = true,
                Data = result
            };
        }
    } 
}
