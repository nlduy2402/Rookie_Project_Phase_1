using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Interfaces;

namespace RetailSystem.CustomerSite.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoryMenuViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }
    }
}
