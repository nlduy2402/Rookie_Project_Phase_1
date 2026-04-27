using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Repository
{
    public class BaseRepository<TModel> : IBaseRepository<TModel> where TModel : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TModel> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TModel>();
        }

        public IQueryable<TModel> Query => _dbSet;

        public async Task<TModel?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

        public async Task<TModel?> GetFirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "")
        {
            IQueryable<TModel> query = _dbSet;
            foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp.Trim());
            }
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<TModel>> GetAllAsync(Expression<Func<TModel, bool>>? filter = null,string includeProperties = "")
        {
            IQueryable<TModel> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp.Trim());
            }

            return await query.ToListAsync();
        }

        public async Task CreateAsync(TModel entity) => await _dbSet.AddAsync(entity);

        public void Update(TModel entity) => _dbSet.Update(entity);

        public void Delete(TModel entity) => _dbSet.Remove(entity);
    }
}
