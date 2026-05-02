using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.ViewModels;

namespace RetailSystem.CustomerSite.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IVNPayService _vnPayService;
        public OrderController(
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                ICartService cartService,
                IOrderService orderService,
                IVNPayService vnPayService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;
            _orderService = orderService;
            _vnPayService = vnPayService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await  _cartService.GetCartAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = cart.Items,
                OrderData = new OrderDTO() 
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var cart = await _cartService.GetCartAsync(userId);
                model.CartItems = cart?.Items ?? new List<CartItem>();

                return View("Checkout", model);
            }
            try
            {
                var userId = _userManager.GetUserId(User);

                var order = await _orderService.CreateOrderAsync(userId, model.OrderData,model.PaymentMethod);

                if (model.PaymentMethod == "COD")
                {
                    await _cartService.ClearCartAsync(userId);

                    TempData["SuccessMessage"] = "Order Success!";
                    return RedirectToAction("Index", "Home");
                }

                // ✅ VNPay
                if (model.PaymentMethod == "VNPay")
                {
                    var url = _vnPayService.CreatePaymentUrl(HttpContext, order);
                    return Redirect(url);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // 4. Xử lý lỗi (hết hàng, lỗi database...)
                ModelState.AddModelError(string.Empty, "Error occured: " + ex.Message);

                // Load lại dữ liệu giỏ hàng để user không thấy trang trắng
                var userId = _userManager.GetUserId(User);
                var cart = await _cartService.GetCartAsync(userId);
                model.CartItems = cart?.Items ?? new List<CartItem>();

                return View("Checkout", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _orderService.GetOrderHistoryAsync(userId);

            return View(orders);
        }

        public async Task<IActionResult> PaymentReturn()
        {
            var result = _vnPayService.Execute(Request.Query);

            if (result == null)
                return View("Fail");

            var order = await _orderService.GetByTxnRefAsync(result.TxnRef);

            if (order == null)
                return View("Fail");

            if (result.Success)
            {
                await _orderService.UpdatePaymentStatusAsync(order, PaymentStatus.Paid);

                await _cartService.ClearCartAsync(order.UserId);

                return View("Success");
            }
            else
            {
                await _orderService.UpdatePaymentStatusAsync(order, PaymentStatus.Failed);

                return View("Fail");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                await _orderService.CancelOrderAsync(id, userId);

                TempData["SuccessMessage"] = "Order cancelled successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("History");
        }
    }
}
