using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.ViewModels;

namespace RetailSystem.CustomerSite.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _service.GetAllAsync();

            var result = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                PriceDisplay = p.Price.ToString("N0") + " VNĐ",
                Status = p.Status.ToString(),
                Categories = p.Categories.Select(c => c.Name).ToList()
            }).ToList();

            return View(result);
        }
        //public ActionResult Index()
        //{
        //    return View("./views/product/test.cshtml");
        //}
    }
}
