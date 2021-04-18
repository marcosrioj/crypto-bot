using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.BackgroundWorker.Trader
{
    public class PredictionWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderService _traderService;

        public PredictionWorker(
            AbpAsyncTimer timer,
            ITraderService traderService)
            : base(timer)
        {
            Timer.Period = 1000;
            _traderService = traderService;
        }

        [UnitOfWork(false)]
        protected override async Task DoWorkAsync()
        {
            try
            {
                await Regression2();
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        private async Task Regression1()
        {
            var interval = KlineInterval.OneMinute;
            var limitOfDataToLearn = 1000;
            var investorProfile = EInvestorProfile.UltraConservative;
            var strategy = EStrategy.SimpleMlStrategy1;

            await _traderService.GenerateBetterPrediction1Async(strategy, investorProfile, interval, limitOfDataToLearn);
        }

        private async Task Regression2()
        {
            var interval = KlineInterval.FifteenMinutes;
            var limitOfDataToLearn = 1000;
            var investorProfile = EInvestorProfile.UltraConservative;
            var strategies = new List<EStrategy>() {
                    EStrategy.SimpleMicrotrendStrategy,
                    EStrategy.SimpleMlStrategy1
                };

            await _traderService.GenerateBetterPrediction2Async(strategies, investorProfile, interval, limitOfDataToLearn);
        }
    }
}
