using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var cart = await _cartService.GetCartAsync(userId);

            List<object> items;

            if (cart == null)
            {
                items = new List<object>();
            }
            else
            {
                items = cart.Items.Select(i => new
                {
                    id = i.ProductId,
                    name = i.Product?.Name,
                    price = i.Product?.Price ?? 0,
                    quantity = i.Quantity,
                    image = i.Product?.Images != null && i.Product.Images.Any()
                        ? i.Product.Images.First().Url.ToString()
                        : "/images/no-image.png"
                }).Cast<object>().ToList();
            }

            return Json(new
            {
                count = cart?.Items.Sum(i => i.Quantity) ?? 0,
                items = items,
                total = cart?.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0)) ?? 0
            });
        }



        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartService.AddToCartAsync(userId, productId, quantity);

            var cart = await _cartService.GetCartAsync(userId);

            return Json(new
            {
                count = cart.Items.Sum(i => i.Quantity),
                items = cart.Items.Select(i => new
                {
                    id = i.ProductId,
                    name = i.Product.Name,
                    price = i.Product.Price,
                    quantity = i.Quantity,
                    image = i.Product.Images.FirstOrDefault()
                }),
                total = cart.Items.Sum(i => i.Quantity * i.Product.Price)
            });
        }
        [HttpPost]
        public async Task<IActionResult> AddAjax(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartService.AddToCartAsync(userId, productId, quantity);

            var cart = await _cartService.GetCartAsync(userId);

            return Json(new
            {
                count = cart.Items.Sum(i => i.Quantity),
                items = cart.Items.Select(i => new
                {
                    id = i.ProductId,
                    name = i.Product.Name,
                    price = i.Product.Price,
                    quantity = i.Quantity,
                    image = i.Product?.Images != null && i.Product.Images.Any()
                ? i.Product.Images.First().Url
                : "/images/no-image.png"
                }),
                total = cart.Items.Sum(i => i.Quantity * i.Product.Price)
            });
        }


        // remove item from cart
        [HttpPost]
        public async Task<IActionResult> RemoveAjax(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartService.RemoveItemAsync(userId, productId);

            var cart = await _cartService.GetCartAsync(userId);

            return Json(new
            {
                count = cart?.Items.Sum(i => i.Quantity) ?? 0,
                items = cart?.Items.Select(i => new
                {
                    id = i.ProductId,
                    name = i.Product?.Name,
                    price = i.Product?.Price ?? 0,
                    quantity = i.Quantity,
                    image = i.Product?.Images != null && i.Product.Images.Any()
                        ? i.Product.Images.First().Url
                        : "/images/no-image.png"
                }),
                total = cart?.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0)) ?? 0
            });
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

            var cart = await _cartService.GetCartAsync(userId);

            return Json(new
            {
                count = cart?.Items.Sum(i => i.Quantity) ?? 0,
                items = cart?.Items.Select(i => new
                {
                    id = i.ProductId,
                    name = i.Product?.Name,
                    price = i.Product?.Price ?? 0,
                    quantity = i.Quantity,
                    image = i.Product?.Images != null && i.Product.Images.Any()
                        ? i.Product.Images.First().Url
                        : "/images/no-image.png"
                }),
                total = cart?.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0)) ?? 0
            });
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
