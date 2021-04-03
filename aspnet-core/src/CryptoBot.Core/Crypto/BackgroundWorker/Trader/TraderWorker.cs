using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Strategies.Simple.MLStrategy;

namespace CryptoBot.Crypto.BackgroundWorker.Trader
{
    public class TraderWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderTestService _traderTestService;

        public TraderWorker(
            AbpAsyncTimer timer,
            ITraderTestService traderTestService)
            : base(timer)
        {
            Timer.Period = 1000;
            _traderTestService = traderTestService;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            await Test2();
        }

        private async Task Test2()
        {
            try
            {
                var initialWallet = 1000;
                var interval = KlineInterval.FifteenMinutes;
                var limitOfDetailsToLearnAndTest = 5000;
                var limitOfDetailsToTest = 120;

                await _traderTestService.RegressionTest(EStrategy.SimpleMlStrategy, ECurrency.ADAUP, interval, initialWallet, ELogLevel.FullLog, limitOfDetailsToLearnAndTest, limitOfDetailsToTest);
                await _traderTestService.RegressionTest(EStrategy.SimpleMicrotrendStrategy, ECurrency.ADAUP, interval, initialWallet, ELogLevel.FullLog, limitOfDetailsToLearnAndTest, limitOfDetailsToTest);
                await _traderTestService.RegressionTest(EStrategy.SimpleMeanReversionStrategy, ECurrency.ADAUP, interval, initialWallet, ELogLevel.FullLog, limitOfDetailsToLearnAndTest, limitOfDetailsToTest);
            }
            catch (Exception e)
            {
                LogHelper.Log($"Worker error: TraderWorker - Message: {e.Message}", $"regression_test-ERROR");
            }
        }

        private async Task Test1()
        {
            var logName = $"complete-regression-test-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";

            try
            {
                var initialWallet = 1000;
                var limitOfDetailsToLearnAndTest = 5000;
                var limitOfDetailsToTest = 120;
                var interval = KlineInterval.FifteenMinutes;

                var result = await _traderTestService.CompleteRegressionTest(interval, initialWallet, limitOfDetailsToLearnAndTest, limitOfDetailsToTest);

                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss K").PadLeft(21, ' ');
                LogHelper.Log($"\n\nDate: {date} - Interval: {interval} - ItemsLearned: {limitOfDetailsToLearnAndTest - limitOfDetailsToTest} - ItemsTested: {limitOfDetailsToTest}", logName);

                ECurrency? currency = null;

                foreach (var item in result)
                {
                    try
                    {
                        var currencyStr = item.Currency.ToString().PadRight(7, ' ');
                        var strategyStr = item.Strategy.ToString().PadRight(27, ' ');
                        var newWalletPriceStr = $"{item.FinalWallet:C2}".PadRight(9, ' ');
                        var newWalletInvestingPriceStr = $"{item.FinalTradingWallet:C2}".PadRight(9, ' ');
                        var percDiff = item.FinalTradingWallet / item.FinalWallet;
                        var percDiffStr = $"{percDiff:P2}";

                        if (currency == null || currency != item.Currency)
                        {
                            LogHelper.Log($"", logName);
                        }

                        LogHelper.Log($"Currency: {currencyStr} - Strategy: {strategyStr} -  FinalWallet: {newWalletPriceStr} - FinalTradingWallet: {newWalletInvestingPriceStr} - PercDiff: {percDiffStr}", logName);

                        currency = item.Currency;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Worker error: TraderWorker - Message: {ex.Message}", $"{logName}-ERROR");
            }
        }
    }
}
