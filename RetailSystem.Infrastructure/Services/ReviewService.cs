using Microsoft.Extensions.Caching.Memory;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
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
    public class ReviewService : BaseService<Review>, IReviewService
    {
        private readonly IUnitOfWork _uow;
        private IMemoryCache _cache;
        public ReviewService(IUnitOfWork uow, IMemoryCache cache) : base(uow, cache)
        {
            _uow = uow;
            _cache = cache;
        }
        protected override IBaseRepository<Review> GetRepository() => _uow.Reviews;
        public async Task<bool> IsReviewedAsync(int productId, int orderId, string userId)
        {
            return await _uow.Reviews.ExistsAsync(productId, orderId, userId);
        }
        public async Task<List<Review>> GetProductReviewsAsync(int productId)
        {
            return await _uow.Reviews.GetByProductIdAsync(productId);
        }

        public async Task CreateReviewsAsync(int orderId, string userId, List<ReviewItemDTO> items)
        {
            var order = await _uow.Orders.GetOrderWithDetailsAsync(orderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.UserId != userId)
                throw new Exception("Unauthorized");

            // ✔ chỉ cho review sau khi hoàn thành
            if (order.Status != OrderStatus.Completed)
                throw new Exception("Order is not completed");

            await _uow.BeginTransactionAsync();

            try
            {
                foreach (var item in items)
                {
                    var inOrder = order.OrderDetails.Any(x => x.ProductId == item.ProductId);

                    if (!inOrder)
                        continue; // hoặc throw

                    var exists = await _uow.Reviews.ExistsAsync(
                        item.ProductId,
                        orderId,
                        userId);

                    if (exists)
                        continue;

                    var review = new Review
                    {
                        ProductId = item.ProductId,
                        OrderId = orderId,
                        UserId = userId,
                        Rating = item.Rating,
                        Comment = item.Comment
                    };

                    await _uow.Reviews.CreateAsync(review);

                    //if (item.ImageUrls != null && item.ImageUrls.Any())
                    //{
                    //    foreach (var url in item.ImageUrls)
                    //    {
                    //        var img = new ReviewImage
                    //        {
                    //            Review = review,
                    //            ImageUrl = url
                    //        };

                    //        await _uow.ReviewImages.CreateAsync(img);
                    //    }
                    //}
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
