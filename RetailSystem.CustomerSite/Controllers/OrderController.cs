using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Domain.Entities;
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
        public OrderController(
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;
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
    }
}
