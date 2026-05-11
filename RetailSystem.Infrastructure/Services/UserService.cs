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
using RetailSystem.Infrastructure.Repository.Interface;

namespace RetailSystem.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<ServiceResult<List<UserDTO>>> GetAllCustomerAccountsAsync()
        {
            var users = await _userRepo.GetAllCustomersAsync();

            var result = users.Select(u => new UserDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email
            }).ToList();

            return new ServiceResult<List<UserDTO>>
            {
                IsSuccess = result.Any(),
                Data = result,
                Message = result.Any() ? null : "No customers found."
            };
        }
    }
}
