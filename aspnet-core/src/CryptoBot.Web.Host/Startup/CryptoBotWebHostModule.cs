using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using CryptoBot.Configuration;
using CryptoBot.Crypto.BackgroundWorker.Worker;
using CryptoBot.Crypto.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CryptoBot.Web.Host.Startup
{
    [DependsOn(
       typeof(CryptoBotWebCoreModule))]
    public class CryptoBotWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public CryptoBotWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.BackgroundJobs.IsJobExecutionEnabled = true;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CryptoBotWebHostModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            workManager.Add(IocManager.Resolve<TestsWorker>());

            //var traderService = IocManager.Resolve<ITraderService>();
            //traderService.ScheduleAutoTraderSellWithWalletVirtualAsync();
            //traderService.StartSchedulePredictions();
            //traderService.StartScheduleRobots();
        }
    }
}
