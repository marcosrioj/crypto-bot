using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CryptoBot.Configuration;

namespace CryptoBot.Web.Host.Startup
{
    [DependsOn(
       typeof(CryptoBotWebCoreModule))]
    public class CryptoBotWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public CryptoBotWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CryptoBotWebHostModule).GetAssembly());
        }
    }
}
