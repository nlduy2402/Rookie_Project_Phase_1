using RetailSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository.Interface
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllCustomersAsync();

        Task<User?> GetByIdAsync(string id);

        Task<bool> IsExistsAsync(string id);

        Task<List<User>> GetByRoleAsync(string role);

        Task CreateAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(User user);
    }
}
