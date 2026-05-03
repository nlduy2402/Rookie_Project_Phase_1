using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace RetailSystem.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        public IProductRepository Products { get; private set; }
        //public IProductRepository Products { get; set; }
        public IBaseRepository<Category> Categories { get; private set; }
        public IBaseRepository<AdminAccount> AdminAccounts { get; private set; }
        public ICartRepository Carts { get; private set; }
        public IOrderRepository Orders { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Products = new ProductRepository(_context);
            Categories = new BaseRepository<Category>(_context);
            AdminAccounts = new BaseRepository<AdminAccount>(_context);
            //Carts = new BaseRepository<Cart>(_context);
            Carts = new CartRepository(_context);
            Orders = new OrderRepository(_context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null!;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null!;
            }
        }
        public void Dispose()
        {
            _context.Dispose();
            _transaction?.Dispose();
        }
    }
}
