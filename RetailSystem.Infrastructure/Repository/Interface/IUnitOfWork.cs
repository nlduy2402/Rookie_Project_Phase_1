using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace RetailSystem.Infrastructure.Repository.Interface
{
    public interface IUnitOfWork
    {
        IBaseRepository<Product> Products { get; }
        IBaseRepository<Category> Categories { get; } 
        Task<int> SaveChangesAsync();
    }
}
