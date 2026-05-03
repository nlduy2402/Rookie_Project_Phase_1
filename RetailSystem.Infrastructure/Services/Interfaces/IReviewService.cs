using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface IReviewService
    {
        Task<bool> IsReviewedAsync(int productId, int orderId, string userId);

        Task CreateReviewsAsync(int orderId, string userId, List<ReviewItemDTO> items);

        Task<List<Review>> GetProductReviewsAsync(int productId);
    }
}
