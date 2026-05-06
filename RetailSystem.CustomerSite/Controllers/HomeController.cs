using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.CustomerSite.Models;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.Extensions;
using RetailSystem.Shared.ViewModels;
namespace RetailSystem.CustomerSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            //var result = await _productService.GetAllProductAsync();
            //if (!result.IsSuccess) return BadRequest(result.Message);

            //var vm = result?.Data?.Select(p => p.ToCardVM()).ToList() ?? new List<ProductViewModel>();

            //return View(vm);

            var result = await _productService.GetTopSellingProductCardsAsync(4);
            if (!result.IsSuccess) return BadRequest(result.Message);

            return View(result.Data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
