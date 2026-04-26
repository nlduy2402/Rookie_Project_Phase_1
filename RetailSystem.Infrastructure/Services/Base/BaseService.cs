using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Base
{
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly IMemoryCache _cache;
        protected readonly string _cachePrefix;
        protected string AllCacheKey => $"{_cachePrefix}All";

        public BaseService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
            _cache = cache;
            _cachePrefix = $"{typeof(TEntity).Name}_";

        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            if (!_cache.TryGetValue(AllCacheKey, out List<TEntity>? entities))
            {
                entities = await _dbSet.ToListAsync();
                _cache.Set(AllCacheKey, entities, TimeSpan.FromMinutes(10));
            }
            return entities ?? new List<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            string key = $"{_cachePrefix}{id}";
            if (!_cache.TryGetValue(key, out TEntity? entity))
            {
                entity = await _dbSet.FindAsync(id);
                if (entity != null)
                {
                    _cache.Set(key, entity, TimeSpan.FromMinutes(10));
                }
            }
            return entity;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            _cache.Remove(AllCacheKey);
            return entity;
        }

        public virtual async Task<string> DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return "Not exist data to delete!";

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();

            _cache.Remove(AllCacheKey);
            _cache.Remove($"{_cachePrefix}{id}");

            return "Deleted!";
        }
        public async Task<List<TEntity>> GetPagedAsync(int page, int pageSize)
        {
            return await _dbSet
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
