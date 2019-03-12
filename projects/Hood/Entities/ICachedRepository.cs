using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hood.Entities
{
    public interface ICachedRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        IQueryable<TEntity> Table { get; }
        IQueryable<TEntity> TableNoTracking { get; }

        Task DeleteAsync(IEnumerable<TEntity> entities);
        Task DeleteAsync(TEntity entity);
        Task<TEntity> FindAsync(TKey id);
        Task InsertAsync(IEnumerable<TEntity> entities);
        Task InsertAsync(TEntity entity);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task UpdateAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
    }
}