using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Base;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services
{
    public class AdminService :  BaseService<AdminAccount>,IAdminService
    {
        private readonly IConfiguration _config;


        public AdminService(AppDbContext context, IConfiguration config) : base(context)
        {
            _config = config;
        }

        public string GenerateToken(AdminAccount admin)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, admin.Role)
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ServiceResult<string>> LoginAsync(LoginDTO model)
        {
            var admin = await _context.AdminAccounts
                    .FirstOrDefaultAsync(x => x.Username == model.Username);

            if (admin == null || admin.PasswordHash != model.Password)
            {
                return new ServiceResult<string>
                {
                    IsSuccess = false,
                    Message = "Invalid username or password"
                };
            }
            var token = GenerateToken(admin);
            return new ServiceResult<string> {
                IsSuccess = true,
                Data = token
            };


        }
    }
}
