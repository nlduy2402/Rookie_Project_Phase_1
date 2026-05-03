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
        IProductRepository Products { get; }
        IBaseRepository<Category> Categories { get; } 
        IBaseRepository<AdminAccount> AdminAccounts { get; }
        ICartRepository Carts { get; }
        IOrderRepository Orders { get; }
        IReviewRepository Reviews { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
