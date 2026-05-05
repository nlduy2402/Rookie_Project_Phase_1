using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Services.Interfaces;

namespace RetailSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseController
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IOrderService _orderService;
        public OrdersController(ILogger<OrdersController> logger, IOrderService orderService) {
            _logger = logger;
            _orderService = orderService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1,[FromQuery] int pageSize=4)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and PageSize must > 0.");
            }
            var result = await _orderService.GetAllOrdersPagedAsync(page, pageSize);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return OkResponse(result.Data);
        }

        [HttpPost("Ship/{id}")]
        public async Task<IActionResult> Ship(int id)
        {
            var result = await _orderService.ShipOrderAsync(id);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return OkResponse(result.Data);

        }
    }
}
