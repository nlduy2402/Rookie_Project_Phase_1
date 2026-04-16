using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.CustomerSite.Models;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;

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

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
