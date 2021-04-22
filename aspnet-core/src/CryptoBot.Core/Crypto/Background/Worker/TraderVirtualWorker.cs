using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.BackgroundWorker.Trader
{
    public class TraderVirtualWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderService _traderService;
        private readonly IBinanceService _binanceService;
        private readonly ITraderTestService _traderTestService;

        public TraderVirtualWorker(
            AbpAsyncTimer timer,
            ITraderService traderService,
            ITraderTestService traderTestService,
            IBinanceService binanceService)
            : base(timer)
        {
            Timer.Period = 1000;
            _traderService = traderService;
            _traderTestService = traderTestService;
            _binanceService = binanceService;
        }

        [UnitOfWork(false)]
        protected override async Task DoWorkAsync()
        {
            try
            {
                var interval = KlineInterval.FiveMinutes;
                var limitOfDataToLearn = 1000;
                var investorProfile = EInvestorProfile.UltraConservative;
                var strategy = EStrategy.SimpleMlStrategy1;
                //var strategies = new List<EStrategy>() {
                //    EStrategy.SimpleMicrotrendStrategy,
                //    //EStrategy.SimpleMlStrategy1
                //};

                //await _traderService.GenerateBetterPrediction1Async(strategy, investorProfile, interval, limitOfDataToLearn);

                //await _traderService.AutoTraderBuyWithWalletVirtualAsync(8);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
