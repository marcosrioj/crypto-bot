using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using CryptoBot.Crypto.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.BackgroundWorker.ComparativeHistorical
{
    public class ComparativeHistoricalDailyWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ILogger<ComparativeHistoricalDailyWorker> _logger;
        private readonly IQuoteService _quoteService;

        public ComparativeHistoricalDailyWorker(
            AbpAsyncTimer timer,
            ILogger<ComparativeHistoricalDailyWorker> logger,
            IQuoteService quoteService)
            : base(timer)
        {
            Timer.Period = 10000;
            _logger = logger;
            _quoteService = quoteService;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            try
            {
                await _quoteService.AddCurrentQuoteHistoryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Worker error: UpdateQuoteHistoryWorker - Message: {ex.Message}");
            }
        }
    }
}
