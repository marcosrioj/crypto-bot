using Abp.Domain.Entities;
using Abp.EntityFrameworkCore.Repositories;
using System.Collections.Generic;

namespace CryptoBot.Crypto.Repositories
{
    public interface ICryptoBotRepositoryBase<TEntity, TPrimaryKey> : IRepositoryWithDbContext
        where TEntity : class, IEntity<TPrimaryKey>
    {
        bool BulkImport(IList<TEntity> entities);
    }

    public interface ICryptoBotRepositoryBase<TEntity> : ICryptoBotRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
    }
}
