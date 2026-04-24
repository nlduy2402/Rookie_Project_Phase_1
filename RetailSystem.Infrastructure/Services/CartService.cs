using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
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
    public class CartService : BaseService<Cart>,ICartService
    {
        public CartService(AppDbContext context) : base(context)
        {

        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Quantity must be > 0");

            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
                throw new Exception("Product not found");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(string userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null) return;

            cart.Items.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null) return;

            item.Quantity = quantity;

            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            cart.Items.Clear();

            await _context.SaveChangesAsync();
        }

        public async Task<CartDTO> GetCartDtoAsync(string userId)
        {
            var cart = await GetCartAsync(userId);

            if (cart == null)
                return new CartDTO();

            return new CartDTO
            {
                Count = cart.Items.Sum(i => i.Quantity),

                Items = cart.Items.Select(i => new CartItemDTO
                {
                    Id = i.ProductId,
                    Name = i.Product?.Name,
                    Price = i.Product?.Price ?? 0,
                    Quantity = i.Quantity,
                    Image = i.Product?.Images != null && i.Product.Images.Any()
                        ? i.Product.Images.First().Url
                        : "/images/no-image.png"
                }).ToList(),

                Total = cart.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0))
            };
        }
    }
}
