using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using CryptoBot.Authorization.Roles;
using CryptoBot.Authorization.Users;
using CryptoBot.MultiTenancy;

namespace CryptoBot.EntityFrameworkCore
{
    public class CryptoBotDbContext : AbpZeroDbContext<Tenant, Role, User, CryptoBotDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public CryptoBotDbContext(DbContextOptions<CryptoBotDbContext> options)
            : base(options)
        {
        }
    }
}
