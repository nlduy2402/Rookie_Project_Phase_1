using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context) { }
        public async Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public void ClearCart(Cart cart)
        {
            if (cart.Items.Any())
            {
                _context.Set<CartItem>().RemoveRange(cart.Items);
            }
        }
    }
}
