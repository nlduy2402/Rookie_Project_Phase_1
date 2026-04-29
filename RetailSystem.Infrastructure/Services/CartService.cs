using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Persistence;
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
    public class CartService : BaseService<Cart>,ICartService
    {
        private readonly new IUnitOfWork _uow;
        public CartService(IUnitOfWork uow,IMemoryCache cache) : base(uow,cache)
        {
            _uow = uow;
        }
        protected override IBaseRepository<Cart> GetRepository() => _uow.Carts;
        
        public async Task<Cart?> GetCartAsync(string userId)
        {
            //return await _context.Carts
            //    .Include(c => c.Items)
            //    .ThenInclude(i => i.Product)
            //    .ThenInclude(p => p.Images)
            //    .FirstOrDefaultAsync(c => c.UserId == userId);

            var cart = await _uow.Carts.GetFirstOrDefaultAsync(
            predicate: c => c.UserId == userId,
            includeProperties: "Items.Product.Images");
            return cart;
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Quantity must be > 0");

            var productExists = await _uow.Products.GetByIdAsync(productId);
            if (productExists == null)
                throw new Exception("Product not found");

            //var cart = await _context.Carts
            //    .Include(c => c.Items)
            //    .FirstOrDefaultAsync(c => c.UserId == userId);
            var cart = await _uow.Carts.GetFirstOrDefaultAsync(c => c.UserId == userId, "Items");

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                //_context.Carts.Add(cart);
                await _uow.Carts.CreateAsync(cart);
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

            //await _context.SaveChangesAsync();
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(string userId, int productId)
        {
            //var cart = await _context.Carts
            //    .Include(c => c.Items)                                                                   
            //    .FirstOrDefaultAsync(c => c.UserId == userId);
            var cart = await _uow.Carts.GetFirstOrDefaultAsync(c => c.UserId == userId, "Items");

            var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null) return;

            cart?.Items.Remove(item);
            //await _context.SaveChangesAsync();
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            //var cart = await _context.Carts
            //    .Include(c => c.Items)
            //    .FirstOrDefaultAsync(c => c.UserId == userId);

            var cart = await _uow.Carts.GetFirstOrDefaultAsync(c => c.UserId == userId, "Items");

            var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null) return;

            item.Quantity = quantity;

            //await _context.SaveChangesAsync();
            await _uow.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            //var cart = await _context.Carts
            //    .Include(c => c.Items)
            //    .FirstOrDefaultAsync(c => c.UserId == userId);
            var cart = await _uow.Carts.GetFirstOrDefaultAsync(c => c.UserId == userId, "Items");

            if (cart == null) return;

            cart.Items.Clear();

            //await _context.SaveChangesAsync();
            await _uow.SaveChangesAsync();
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
