using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<Order> CreateOrderAsync(string userId, OrderDTO model, string PaymentMethod);
        //Task<IEnumerable<Order>> GetOrderHistoryAsync(string userId);
        Task<Order?> GetByTxnRefAsync(string txnRef);
        Task UpdatePaymentStatusAsync(Order order, PaymentStatus paymentStatus);
        Task CancelOrderAsync(int orderId, string userId);
        Task<ServiceResult<PageResult<Order>>> GetUserOrdersPagedAsync(string userId, int page, int pageSize);
        Task<Order> GetOrderWithDetailsAsync(int orderId, string userId);
        //Task ShipOrderAsync(int orderId, string userId);
        Task<ServiceResult<string>> ShipOrderAsync(int orderId);
        Task CompleteOrderAsync(int orderId, string userId);

        Task<ServiceResult<PageResult<Order>>> GetAllOrdersPagedAsync(int page, int pageSize);

    }
}
