using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<bool> ExistsAsync(int productId, int orderId, string userId)
        {
            return await _context.Reviews.AnyAsync(r =>
                r.ProductId == productId &&
                r.OrderId == orderId &&
                r.UserId == userId);
        }
        public async Task<List<Review>> GetByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
