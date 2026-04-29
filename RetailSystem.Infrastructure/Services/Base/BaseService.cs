using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Persistence;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Base
{
    public abstract class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {

        protected readonly IUnitOfWork _uow;
        protected readonly IMemoryCache _cache;
        protected readonly string _cachePrefix;
        protected string AllCacheKey => $"{_cachePrefix}All";

        public BaseService(IUnitOfWork uow, IMemoryCache cache)
        {
            _cache = cache;
            _cachePrefix = $"{typeof(TEntity).Name}_";
            _uow = uow;

        }
        protected abstract IBaseRepository<TEntity> GetRepository();
        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            if (!_cache.TryGetValue(AllCacheKey, out List<TEntity>? entities))
            {
                var result = await GetRepository().GetAllAsync();
                entities = result.ToList();
                _cache.Set(AllCacheKey, entities, TimeSpan.FromMinutes(10));
            }
            return entities ?? new List<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            string key = $"{_cachePrefix}{id}";
            if (!_cache.TryGetValue(key, out TEntity? entity))
            {
                entity = await GetRepository().GetByIdAsync(id);
                if (entity != null)
                {
                    _cache.Set(key, entity, TimeSpan.FromMinutes(10));
                }
            }
            return entity;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            await GetRepository().CreateAsync(entity);
            await _uow.SaveChangesAsync();

            _cache.Remove(AllCacheKey);
            return entity;
        }

        public virtual async Task<string> DeleteAsync(int id)
        {
            var entity = await GetRepository().GetByIdAsync(id);
            if (entity == null) return "Not exist data to delete!";

            GetRepository().Delete(entity);
            await _uow.SaveChangesAsync();

            _cache.Remove(AllCacheKey);
            _cache.Remove($"{_cachePrefix}{id}");

            return "Deleted!";
        }
        //public async Task<List<TEntity>> GetPagedAsync(int page, int pageSize)
        //{
        //    return await _dbSet
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //}
    }
}
