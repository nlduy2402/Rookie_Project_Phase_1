using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.API.Shared;
using RetailSystem.Shared;
namespace RetailSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected string CurrentTraceId => HttpContext.TraceIdentifier;

        protected IActionResult OkResponse<T>(T data, string message = "Success")
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                TraceId = CurrentTraceId
            };

            return Ok(response);
        }
    }
}
