using Abp.Modules;
using Abp.Quartz;
using Abp.Reflection.Extensions;
using CryptoBot.Configuration;
using CryptoBot.Crypto.Background.Jobs;
using CryptoBot.Crypto.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Quartz;

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
            //var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            //workManager.Add(IocManager.Resolve<TestsWorker>());

            StartupJobs();
        }
        private void StartupJobs()
        {
            InitializeSellTraderAsync();
            StartFormulasAsync();
        }

        private void InitializeSellTraderAsync()
        {
            var jobManager = IocManager.Resolve<IQuartzScheduleJobManager>();

            jobManager.ScheduleAsync<SellVirtualTraderJob>(
                job =>
                {
                    job.WithIdentity("SellTrader");
                },
                trigger =>
                {
                    trigger.StartNow()
                        .WithSimpleSchedule(schedule =>
                        {
                            schedule.RepeatForever()
                                .WithIntervalInSeconds(1)
                                .Build();
                        });
                });
        }

        private void StartFormulasAsync()
        {
            var traderService = IocManager.Resolve<ITraderService>();
            traderService.StartScheduleFormulas();
        }
    }
}
