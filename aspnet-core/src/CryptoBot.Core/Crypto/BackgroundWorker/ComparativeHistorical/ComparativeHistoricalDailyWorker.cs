using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.BackgroundWorker.ComparativeHistorical
{
    public class ComparativeHistoricalDailyWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ILogger<ComparativeHistoricalDailyWorker> _logger;
        private readonly IComparativeHistoricalService _comparativeHistoricalService;

        public ComparativeHistoricalDailyWorker(
            AbpAsyncTimer timer,
            ILogger<ComparativeHistoricalDailyWorker> logger,
            IComparativeHistoricalService comparativeHistoricalService)
            : base(timer)
        {
            Timer.Period = 10000;
            _logger = logger;
            _comparativeHistoricalService = comparativeHistoricalService;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            try
            {
                var approachTrading = EApproachTrading.SimplePriceVariation;
                var currency = ECurrency.ANKR;
                var interval = KlineInterval.OneMinute;
                var limitOfDetails = 10;

                var result = await _comparativeHistoricalService.GenerateGroupComparativeHistorical(
                    approachTrading,
                    currency,
                    interval,
                    limitOfDetails
                    );
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Worker error: ComparativeHistoricalDailyWorker - Message: {ex.Message}");
            }
        }
    }
}
