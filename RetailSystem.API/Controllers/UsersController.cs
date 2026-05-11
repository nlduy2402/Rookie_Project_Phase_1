using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;

namespace RetailSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllCustomerAccountsAsync();
            if (result.IsSuccess)
            {
                return OkResponse(result.Data);
            }
            else
            {
                return NotFound(result.Message);
            }
        }
    }
}
