using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using System.Security.Claims;

namespace RetailSystem.CustomerSite.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }


        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            return View(cart);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartJson()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _cartService.GetCartDtoAsync(userId);

            return Json(result);
        }



        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = GetUserId();

            await _cartService.AddToCartAsync(userId, productId, quantity);

            return Json(await _cartService.GetCartDtoAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> AddAjax(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartService.AddToCartAsync(userId, productId, quantity);

            var cart = await _cartService.GetCartAsync(userId);

            return Json(await _cartService.GetCartDtoAsync(userId));
        }


        // remove item from cart
        [HttpPost]
        public async Task<IActionResult> RemoveAjax(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartService.RemoveItemAsync(userId, productId);

            return Json(await _cartService.GetCartDtoAsync(userId));
        }

        //update item quantity in cart
        [HttpPost]
        public async Task<IActionResult> UpdateQuantityAjax(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (quantity <= 0)
            {
                await _cartService.RemoveItemAsync(userId, productId);
            }
            else
            {
                await _cartService.UpdateQuantityAsync(userId, productId, quantity);
            }

            return Json(await _cartService.GetCartDtoAsync(userId));
        }


        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            await _cartService.RemoveItemAsync(GetUserId(), productId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            await _cartService.UpdateQuantityAsync(GetUserId(), productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _cartService.ClearCartAsync(GetUserId());
            return RedirectToAction("Index");
        }
    }
}
