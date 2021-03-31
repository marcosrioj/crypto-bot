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
        private readonly ICurrencyService _currencyService;

        public ComparativeHistoricalDailyWorker(
            AbpAsyncTimer timer,
            ILogger<ComparativeHistoricalDailyWorker> logger,
            IComparativeHistoricalService comparativeHistoricalService,
            ICurrencyService currencyService)
            : base(timer)
        {
            Timer.Period = 10000;
            _logger = logger;
            _comparativeHistoricalService = comparativeHistoricalService;
            _currencyService = currencyService;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            try
            {
                var approachTrading = EApproachTrading.SimplePriceVariation;
                var interval = KlineInterval.OneMinute;
                var limitOfDetails = 10;
                var coins = await _currencyService.GetActiveCurrencies();

                foreach (var coin in coins)
                {
                    await _comparativeHistoricalService.GenerateGroupComparativeHistorical(
                                        approachTrading,
                                        coin,
                                        interval,
                                        limitOfDetails
                                        );
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Worker error: ComparativeHistoricalDailyWorker - Message: {ex.Message}");
            }
        }
    }
}
