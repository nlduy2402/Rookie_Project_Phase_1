using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _context;

        public TokenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveTokenAsync(User user, string accessToken, string refreshToken)
        {
            var token = new UserToken
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiredAt = DateTime.UtcNow.AddHours(1)
            };

            _context.UserTokens.Add(token);
            await _context.SaveChangesAsync();
        }
    }
}
