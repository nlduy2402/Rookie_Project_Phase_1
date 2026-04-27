using RetailSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace RetailSystem.Domain.Repository.Interface
{
    public interface IBaseRepository<TModel> where TModel : class
    {
        IQueryable<TModel> Query { get; }

        Task<TModel?> GetByIdAsync(object id);

        Task<TModel?> GetFirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "");

        Task<IEnumerable<TModel>> GetAllAsync(Expression<Func<TModel, bool>>? filter = null,string includeProperties = "");

        Task CreateAsync(TModel entity);

        void Update(TModel entity);

        void Delete(TModel entity);
    }
}
