using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CryptoBot.Configuration;
using Abp.Threading.BackgroundWorkers;
using CryptoBot.Crypto.BackgroundWorker.Trader;

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
            //workManager.Add(IocManager.Resolve<TraderBuyWorker>());
            //workManager.Add(IocManager.Resolve<PredictionWorker>());

            //workManager.Add(IocManager.Resolve<Prediction3Worker>());
            //workManager.Add(IocManager.Resolve<Prediction4Worker>());
            workManager.Add(IocManager.Resolve<Prediction5Worker>());
            workManager.Add(IocManager.Resolve<Prediction6Worker>());
            //workManager.Add(IocManager.Resolve<Prediction7Worker>());
            //workManager.Add(IocManager.Resolve<Prediction8Worker>());

            //workManager.Add(IocManager.Resolve<TraderBuyUser3Worker>());
            //workManager.Add(IocManager.Resolve<TraderBuyUser4Worker>());
            workManager.Add(IocManager.Resolve<TraderBuyUser5Worker>());
            workManager.Add(IocManager.Resolve<TraderBuyUser6Worker>());
            //workManager.Add(IocManager.Resolve<TraderBuyUser7Worker>());
            //workManager.Add(IocManager.Resolve<TraderBuyUser8Worker>());

            workManager.Add(IocManager.Resolve<TraderSellWorker>());
        }
    }
}
