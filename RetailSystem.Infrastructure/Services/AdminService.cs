using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Models;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Base;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RetailSystem.Shared.Helpers;
using System.Reflection.Metadata;


namespace RetailSystem.Infrastructure.Services
{
    public class AdminService :  BaseService<AdminAccount>,IAdminService
    {
        private readonly IConfiguration _config;


        public AdminService(AppDbContext context,IMemoryCache cache, IConfiguration config) : base(context,cache)
        {
            _config = config;
        }

        public string GenerateToken(AdminAccount acc)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, acc.Username),
                new Claim(ClaimTypes.Role, acc.Role),
                new Claim(ClaimTypes.NameIdentifier, acc.Id.ToString()) 
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), 
                SigningCredentials = creds,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };

            return refreshToken;
        }

        public async Task<ServiceResult<AdminAccount>> RegisterAsync(LoginDTO request)
        {
            var accountExists = await _context.AdminAccounts.AnyAsync(x => x.Username == request.Username);
            if (accountExists)
            {
                return new ServiceResult<AdminAccount>
                {
                    IsSuccess = false,
                    Message = "Username already exists"
                };
            }
            AdminAccount admin = new AdminAccount
            {
                Username = request.Username,
                Role = "Admin"
            };
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            admin.PasswordHash = ByteConvert.ByteArrayToString(passwordHash);
            admin.PasswordSalt = ByteConvert.ByteArrayToString(passwordSalt);
            var token = GenerateToken(admin);
            var refreshToken = GenerateRefreshToken();
            await SetRefreshToken(refreshToken, admin);


            _context.AdminAccounts.Add(admin);
            await _context.SaveChangesAsync();
            return new ServiceResult<AdminAccount>
            {
                IsSuccess = true,
                Data = admin
            };
        }

        public async Task<ServiceResult<string>> LoginAsync(LoginDTO model)
        {
            var admin = await _context.AdminAccounts
                    .FirstOrDefaultAsync(x => x.Username == model.Username);

            if (admin == null || 
                !VerifyPasswordHash(model.Password, 
                ByteConvert.StringToByteArray(admin.PasswordHash), 
                ByteConvert.StringToByteArray(admin.PasswordSalt))
                )
            {
                return new ServiceResult<string>
                {
                    IsSuccess = false,
                    Message = "Invalid username or password"
                };
            }
            string token = GenerateToken(admin);

            var refreshToken = GenerateRefreshToken();
            await SetRefreshToken(refreshToken, admin);
            return new ServiceResult<string> {
                IsSuccess = true,
                Data = token
            };
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public async Task SetRefreshToken(RefreshToken newRefreshToken, AdminAccount admin)
        {
            var user = await _context.AdminAccounts.FindAsync(admin.Id);
            if (user == null) return;

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;

            await _context.SaveChangesAsync();
        }
    }
}
