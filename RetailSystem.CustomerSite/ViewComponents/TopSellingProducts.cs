using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Services.Interfaces;

namespace RetailSystem.CustomerSite.ViewComponents
{
    public class TopSellingProductsViewComponent : ViewComponent
    {
        private readonly IProductService _productService;

        public TopSellingProductsViewComponent(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int top = 4)
        {
            var result = await _productService.GetTopSellingProductCardsAsync(top);

            if (!result.IsSuccess) return Content("Không thể tải sản phẩm");

            return View(result.Data);
        }
    }
}
