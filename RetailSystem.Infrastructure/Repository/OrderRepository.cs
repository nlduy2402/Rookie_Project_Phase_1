using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Shared.Extensions;
using RetailSystem.Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {

        }
        public async Task<(IEnumerable<Order>, int totalCount)> GetUserOrdersPagedAsync(
        string userId, int page, int pageSize)
            {
                var query = _context.Orders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .AsQueryable();

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                    .ToListAsync();

                return (items, totalCount);
            }
        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product).ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        public async Task<PageResult<Order>> GetOrderHistoryByUserIdAsync(
            string userId, int page, int pageSize)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToPagedAsync(page, pageSize);
        }
    }
}
