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

namespace RetailSystem.Infrastructure.Services
{
    public class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IUnitOfWork _uow;
        private IMemoryCache _cache;
        public OrderService(IUnitOfWork uow, IMemoryCache cache) : base(uow,cache)
        {
            _uow = uow;
            _cache = cache;
        }
        protected override IBaseRepository<Order> GetRepository() => _uow.Orders;

        public async Task<Order> CreateOrderAsync(string userId, OrderDTO orderDto)
        {
            // 1. Lấy giỏ hàng
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
                    TotalAmount = cart.Items.Sum(x => x.Quantity * x.Product.Price)
                };

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
                _uow.Carts.ClearCart(cart);


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
    }
}
