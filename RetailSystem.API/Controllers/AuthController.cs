using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RetailSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAdminService _adminService;

        public AuthController(IConfiguration config, IAdminService adminService)
        {
            _config = config;
            _adminService = adminService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var result = await _adminService.LoginAsync(model);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new
            {
                token = result.Data
            });
        }



    }
}
