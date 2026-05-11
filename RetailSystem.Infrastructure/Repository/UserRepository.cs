using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<User>> GetAllCustomersAsync()
        {
            return await _userManager.Users
                .Where(u => u.Email != null)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<bool> IsExistsAsync(string id)
        {
            return await _userManager.FindByIdAsync(id) != null;
        }

        public async Task<List<User>> GetByRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.ToList();
        }

        public async Task CreateAsync(User user)
        {
            await _userManager.CreateAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task DeleteAsync(User user)
        {
            await _userManager.DeleteAsync(user);
        }
    }
}
