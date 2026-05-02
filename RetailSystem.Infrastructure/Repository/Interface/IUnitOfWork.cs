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
        IBaseRepository<AdminAccount> AdminAccounts { get; }
        //IBaseRepository<Cart> Carts { get; }
        ICartRepository Carts { get; }
        //IBaseRepository<Order> Orders { get; }
        IOrderRepository Orders { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
