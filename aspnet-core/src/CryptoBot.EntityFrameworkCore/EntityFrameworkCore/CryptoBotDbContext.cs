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
        public DbSet<Order> Orders { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<Prediction> Predictions { get; set; }

        public DbSet<PredictionOrder> PredictionOrders { get; set; }

        public CryptoBotDbContext(DbContextOptions<CryptoBotDbContext> options)
            : base(options)
        {

        }
    }
}
