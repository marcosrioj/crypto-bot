using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace CryptoBot.EntityFrameworkCore
{
    public static class CryptoBotDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<CryptoBotDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<CryptoBotDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
