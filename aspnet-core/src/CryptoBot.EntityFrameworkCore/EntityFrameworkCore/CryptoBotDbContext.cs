using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using CryptoBot.Authorization.Roles;
using CryptoBot.Authorization.Users;
using CryptoBot.Crypto.Entities;
using CryptoBot.MultiTenancy;

namespace CryptoBot.EntityFrameworkCore
{
    public class CryptoBotDbContext : AbpZeroDbContext<Tenant, Role, User, CryptoBotDbContext>
    {
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<QuoteHistory> QuotationHistories { get; set; }
        public DbSet<ComparativeHistorical> ComparativeHistoricals { get; set; }
        public DbSet<ComparativeHistoricalDetail> ComparativeHistoricalDetails { get; set; }
        public DbSet<GroupComparativeHistorical> GroupComparativeHistoricals { get; set; }

        public CryptoBotDbContext(DbContextOptions<CryptoBotDbContext> options)
            : base(options)
        {

        }
    }
}
