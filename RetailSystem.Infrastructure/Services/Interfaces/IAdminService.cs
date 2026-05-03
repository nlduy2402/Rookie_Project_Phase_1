using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Models;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface IAdminService : IBaseService<AdminAccount>
    {
        string GenerateToken(AdminAccount admin);
        Task<ServiceResult<string>> LoginAsync(LoginDTO model);
        Task<ServiceResult<AdminAccount>> RegisterAsync(LoginDTO request);
        Task SetRefreshToken(RefreshToken newRefreshToken, AdminAccount admin);
    }
}
