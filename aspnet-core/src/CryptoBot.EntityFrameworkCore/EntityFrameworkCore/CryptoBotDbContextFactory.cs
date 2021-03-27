using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using CryptoBot.Configuration;
using CryptoBot.Web;

namespace CryptoBot.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class CryptoBotDbContextFactory : IDesignTimeDbContextFactory<CryptoBotDbContext>
    {
        public CryptoBotDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CryptoBotDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            CryptoBotDbContextConfigurer.Configure(builder, configuration.GetConnectionString(CryptoBotConsts.ConnectionStringName));

            return new CryptoBotDbContext(builder.Options);
        }
    }
}
