using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.ViewModels;

namespace RetailSystem.CustomerSite.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IReviewService _reviewService;
        public ReviewController(
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                ICartService cartService,
                IOrderService orderService,
                IReviewService reviewService
                )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;
            _orderService = orderService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Rate(int id, int productId)
        {
            if (id <= 0 || productId <= 0)
                return BadRequest("Invalid input !");
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var order = await _orderService.GetOrderWithDetailsAsync(id, userId);
            if (order == null)
                return NotFound();
            if (order.Status != OrderStatus.Completed)
                return BadRequest("Order not eligible for review");

            var item = order.OrderDetails
                .FirstOrDefault(x => x.ProductId == productId);
            if (item == null)
                return NotFound();
            var vm = new ReviewViewModel
            {
                OrderId = id,
                ProductId = productId,
                ProductName = item.Product.Name,
                ImageUrl = item.Product.Images.FirstOrDefault().Url
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(ReviewViewModel vm)
        {
            var userId = _userManager.GetUserId(User);

            var exists = await _reviewService.IsReviewedAsync(
                vm.ProductId,
                vm.OrderId,
                userId);

            if (exists)
            {
                TempData["ErrorMessage"] = "You have already reviewed this product!";
                return RedirectToAction("History", "Order");
            }
                

            await _reviewService.CreateReviewsAsync(vm.OrderId, userId, new List<ReviewItemDTO>
            {
                new ReviewItemDTO
                {
                    ProductId = vm.ProductId,
                    Rating = vm.Rating,
                    Comment = vm.Comment
                }
            });

            return RedirectToAction("History","Order");
        }
        [HttpGet]
        public async Task<IActionResult> CheckReviewed(int orderId, int productId)
        {
            if (orderId <= 0 || productId <= 0)
                return Json(new { isReviewed = false });
            var userId = _userManager.GetUserId(User);

            var exists = await _reviewService.IsReviewedAsync(productId, orderId, userId);

            return Json(new
            {
                isReviewed = exists
            });
        }
    }
}
