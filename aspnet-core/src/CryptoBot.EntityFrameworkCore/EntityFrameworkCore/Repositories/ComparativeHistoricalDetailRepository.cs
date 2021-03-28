using Abp.EntityFrameworkCore;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Repositories;

namespace CryptoBot.EntityFrameworkCore.Repositories
{
    public class ComparativeHistoricalDetailRepository : CryptoBotRepositoryBase<ComparativeHistoricalDetail, long>, IComparativeHistoricalDetailRepository
    {
        public ComparativeHistoricalDetailRepository(IDbContextProvider<CryptoBotDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }
    }
}
