using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;
using CryptoBot.Crypto.Repositories;
using EFCore.BulkExtensions;
using System.Collections.Generic;

namespace CryptoBot.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// Base class for custom repositories of the application.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
    public abstract class CryptoBotRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<CryptoBotDbContext, TEntity, TPrimaryKey>, ICryptoBotRepositoryBase<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        protected CryptoBotRepositoryBase(IDbContextProvider<CryptoBotDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public bool BulkImport(IList<TEntity> entities)
        {
            try
            {
                var dbContext = GetDbContext();
                dbContext.BulkInsert(entities);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Add your common methods for all repositories
    }

    /// <summary>
    /// Base class for custom repositories of the application.
    /// This is a shortcut of <see cref="CryptoBotRepositoryBase{TEntity,TPrimaryKey}"/> for <see cref="int"/> primary key.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public abstract class CryptoBotRepositoryBase<TEntity> : CryptoBotRepositoryBase<TEntity, int>, IRepository<TEntity>
        where TEntity : class, IEntity<int>
    {
        protected CryptoBotRepositoryBase(IDbContextProvider<CryptoBotDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        // Do not add any method here, add to the class above (since this inherits it)!!!
    }
}
