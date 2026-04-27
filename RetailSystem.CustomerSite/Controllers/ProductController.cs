using Microsoft.AspNetCore.Mvc;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.Extensions;
using RetailSystem.Shared.ViewModels;

namespace RetailSystem.CustomerSite.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _productService.GetAllProductAsync();
            if (!result.IsSuccess) return BadRequest(result.Message);
            var vm = result?.Data?.Select(p => p.ToCardVM()).ToList();

            return View(vm);
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

        public async Task<IActionResult> ByCategory(int id)
        {
            var categoryResult = await _categoryService.GetByIdAsync(id);
            Console.WriteLine(categoryResult.Data?.Name);
            ViewBag.CategoryName = categoryResult.Data?.Name;
            ViewBag.CategoryDescription = categoryResult.Data?.Description;

            var result = await _productService.GetByCategory(id);
            if (!result.IsSuccess)
            {
                return View(result.Message);
            }
            if(result.Data == null || result.Data.Count == 0)
            {
                return View(new List<ProductViewModel>());
            }
            List<ProductViewModel> productsVm = new List<ProductViewModel>();
            foreach (var item in result.Data)
            {
                productsVm.Add(item.ToCardVM());
            }

            return View(productsVm);
        }
        //public ActionResult Index()
        //{
        //    return View("./views/product/test.cshtml");
        //}
    }
}
