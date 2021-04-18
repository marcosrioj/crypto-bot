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
    public class TraderBuyUser6Worker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderService _traderService;
        private readonly IBinanceService _binanceService;
        private readonly ITraderTestService _traderTestService;

        public TraderBuyUser6Worker(
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
                await _traderService.AutoTraderBuyWithWalletVirtualAsync(
                    6,
                    KlineInterval.ThreeMinutes,
                    EInvestorProfile.UltraConservative,
                    EStrategy.SimpleMlStrategy1);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
