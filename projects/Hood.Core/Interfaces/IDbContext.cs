using Hood.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Hood.Interfaces
{
    public interface IDbContext
    {
        #region Methods

        DbSet<TEntity> Set<TEntity, TKey>() where TEntity : BaseEntity<TKey>;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        #endregion
    }
}
