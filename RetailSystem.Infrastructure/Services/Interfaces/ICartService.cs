using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface ICartService : IBaseService<Cart>
    {
        Task AddToCartAsync(string userId, int productId, int quantity);
        Task<Cart?> GetCartAsync(string userId);
        Task RemoveItemAsync(string userId, int productId);
        Task UpdateQuantityAsync(string userId, int productId, int quantity);
        Task ClearCartAsync(string userId);
        Task<CartDTO> GetCartDtoAsync(string userId);
    }
}
