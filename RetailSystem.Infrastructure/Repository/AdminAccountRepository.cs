using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository
{
    public class AdminAccountRepository : BaseRepository<AdminAccount>, IAdminAccountRepository
    {
       AdminAccountRepository(AppDbContext context) : base(context)
        {
        }
    }
}
