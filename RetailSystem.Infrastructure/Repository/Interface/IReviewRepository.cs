using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository.Interface
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<bool> ExistsAsync(int productId, int orderId, string userId);

        Task<List<Review>> GetByProductIdAsync(int productId);
    }
}
