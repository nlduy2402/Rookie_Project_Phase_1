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

namespace RetailSystem.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IBaseRepository<Product> Products { get; private set; }
        public IBaseRepository<Category> Categories { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Products = new BaseRepository<Product>(_context);
            Categories = new BaseRepository<Category>(_context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
