using Microsoft.AspNetCore.Identity;
using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetailSystem.Shared.ResponseModels;
using RetailSystem.Infrastructure.Services.Interfaces;

namespace RetailSystem.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ServiceResult<List<UserDTO>>> GetAllCustomerAccountsAsync()
        {
            var result = await _userManager.Users
              .Select(u => new UserDTO
              {
                  Id = u.Id,
                  UserName = u.UserName,
                  FullName = u.FullName,
                  Email = u.Email
              })
              .ToListAsync();
            if (result != null)
            {
                return new ServiceResult<List<UserDTO>>
                {
                    IsSuccess = true,
                    Data = result
                };
            }
            else
            {
                return new ServiceResult<List<UserDTO>>
                {
                    IsSuccess = false,
                    Message = "No customers found."
                };
            }
        }
    }
}
