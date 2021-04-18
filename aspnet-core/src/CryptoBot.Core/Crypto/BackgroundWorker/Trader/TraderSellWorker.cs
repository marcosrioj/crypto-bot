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
    public class TraderSellWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderService _traderService;
        private readonly IWalletService _walletService;
        private readonly ITraderTestService _traderTestService;

        public TraderSellWorker(
            AbpAsyncTimer timer,
            ITraderService traderService,
            ITraderTestService traderTestService,
            IWalletService walletService)
            : base(timer)
        {
            Timer.Period = 1000;
            _traderService = traderService;
            _traderTestService = traderTestService;
            _walletService = walletService;
        }

        [UnitOfWork(false)]
        protected override async Task DoWorkAsync()
        {
            try
            {
                await _traderService.AutoTraderSellWithWalletVirtualAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
