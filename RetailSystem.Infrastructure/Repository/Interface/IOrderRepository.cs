using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository.Interface
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        //Task<IEnumerable<Order>> GetOrderHistoryByUserIdAsync(string userId);
        Task<PageResult<Order>> GetOrderHistoryByUserIdAsync(string userId, int page, int pageSize);

        Task<PageResult<Order>> GetAllOrdersPagedAsync(int page, int pageSize);

    }
}
