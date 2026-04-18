using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.Extensions;
using RetailSystem.Shared.ViewModels;

namespace RetailSystem.CustomerSite.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            var result = products.Select(p => new ProductViewModel
            {
                Name = p.Name,
            }).ToList();

            return View(result);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.IsSuccess)
            {
                return View("Error");
            }
            var vm = result?.Data?.ToDetailVM();
            return View(vm);

        }
        //public ActionResult Index()
        //{
        //    return View("./views/product/test.cshtml");
        //}
    }
}
