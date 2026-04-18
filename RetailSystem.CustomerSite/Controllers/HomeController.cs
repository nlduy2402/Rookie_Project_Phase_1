using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.CustomerSite.Models;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.Extensions;
namespace RetailSystem.CustomerSite.Controllers
{
    public class HomeController : Controller
    {
        //private readonly AppDbContext _context;

        //public HomeController(AppDbContext context)
        //{
        //    _context = context;
        //}

        //public IActionResult Index()
        //{
        //    try
        //    {
        //        //_context.Products.Add(new Product
        //        //{
        //        //    Name = "Test",
        //        //    Description = "Test",
        //        //    Price = 100,
        //        //    Quantity = 10,
        //        //    CreatedAt = DateTime.UtcNow,
        //        //    LastUpdatedAt = DateTime.UtcNow
        //        //});

        //        //_context.SaveChanges();
        //        var products = _context.Products;
        //        foreach(var product in products)
        //        {
        //            Console.WriteLine(product.Id);
        //            Console.WriteLine(product.Name);
        //            Console.WriteLine(product.Description);
        //            Console.WriteLine(product.Quantity);
        //        }
        //    }
        //    catch (Exception ex) {
        //        Console.WriteLine(ex.InnerException?.Message);

        //    }
        //    return Content("Inserted!");
        //}
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _productService.GetAllProductAsync();
            if (!result.IsSuccess) return BadRequest(result.Message);

            var vm = result?.Data?.Select(p => p.ToCardVM()).ToList();

            return View(vm);
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
