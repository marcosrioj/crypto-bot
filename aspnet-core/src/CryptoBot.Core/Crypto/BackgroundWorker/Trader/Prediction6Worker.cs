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
    public class Prediction6Worker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderService _traderService;

        public Prediction6Worker(
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
                var interval = KlineInterval.ThreeMinutes;
                var limitOfDataToLearn = 1000;
                var investorProfile = EInvestorProfile.UltraConservative;
                var strategy = EStrategy.NormalMlStrategy2;
                //var strategies = new List<EStrategy>() {
                //    EStrategy.SimpleMicrotrendStrategy,
                //    //EStrategy.SimpleMlStrategy1
                //};

                await _traderService.GenerateBetterPrediction1Async(strategy, investorProfile, interval, limitOfDataToLearn);
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}
