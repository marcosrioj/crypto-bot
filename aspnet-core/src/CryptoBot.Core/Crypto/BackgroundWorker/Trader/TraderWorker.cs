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
    public class TraderWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderService _traderService;
        private readonly ITraderTestService _traderTestService;

        public TraderWorker(
            AbpAsyncTimer timer,
            ITraderService traderService,
            ITraderTestService traderTestService)
            : base(timer)
        {
            Timer.Period = 1000;
            _traderService = traderService;
            _traderTestService = traderTestService;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            await Test2();
        }

        private async Task Test4()
        {
            try
            {
                var initialWallet = 1000;
                var interval = KlineInterval.FifteenMinutes;
                var limitOfDataToLearnAndTest = 1000;
                var strategies = new List<EStrategy>() {
                    EStrategy.SimpleMlStrategy1,
                    EStrategy.NormalMlStrategy1,
                    EStrategy.NormalMlStrategy2
                };

                var result = await _traderService.GetBetterCoinsToTraderRightNowAsync(strategies, interval, initialWallet, limitOfDataToLearnAndTest, ELogLevel.FullLog);
            }
            catch (Exception e)
            {
                LogHelper.Log($"Worker error: TraderWorker - Message: {e.Message}", $"regression_test-ERROR");
            }
        }

        private async Task Test3()
        {
            try
            {
                var initialWallet = 1000;
                var interval = KlineInterval.FifteenMinutes;
                var limitOfDataToLearnAndTest = 1000;
                var strategy = EStrategy.SimpleMlStrategy1;

                DateTime start = DateTime.UtcNow;
                var result = await _traderService.GetBetterCoinsToTraderRightNowAsync(strategy, interval, initialWallet, limitOfDataToLearnAndTest);
                var messageResult1 = LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, strategy, result);

                var result2 = await _traderService.FilterBetterCoinsToTraderRightNowAsync(EStrategy.NormalMlStrategy1, result);
                var messageResult2 = LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, EStrategy.NormalMlStrategy1, result2);

                var result3 = await _traderService.FilterBetterCoinsToTraderRightNowAsync(EStrategy.NormalMlStrategy2, result2);
                var messageResult3 = LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, EStrategy.NormalMlStrategy2, result3);

                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;
                var seconds = timeDiff.TotalSeconds;

                messageResult3.AppendLine($"\nTimeExecution: {seconds} seconds");

                LogHelper.Log($"{messageResult1}\n\n{messageResult2}\n\n{messageResult3}\n", "get-better-coins-to-trader-right-now");

            }
            catch (Exception e)
            {
                LogHelper.Log($"Worker error: TraderWorker - Message: {e.Message}", $"regression_test-ERROR");
            }
        }

        private async Task Test2()
        {
            try
            {
                var initialWallet = 1000;
                var interval = KlineInterval.OneMinute;
                var limitOfDataToLearnAndTest = 1000;
                var limitOfDataToTest = 120;

                var data = _traderService.GetRegressionData(ECurrency.BTC, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderService.RegressionExec(EStrategy.SimpleMlStrategy1, data, ELogLevel.FullLog);

                data = _traderService.GetRegressionData(ECurrency.BTC, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderService.RegressionExec(EStrategy.NormalMlStrategy1, data, ELogLevel.FullLog);

                data = _traderService.GetRegressionData(ECurrency.BTC, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderService.RegressionExec(EStrategy.NormalMlStrategy2, data, ELogLevel.FullLog);

                data = _traderService.GetRegressionData(ECurrency.BTC, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderService.RegressionExec(EStrategy.SimpleMeanReversionStrategy, data, ELogLevel.FullLog);

                data = _traderService.GetRegressionData(ECurrency.BTC, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderService.RegressionExec(EStrategy.SimpleMicrotrendStrategy, data, ELogLevel.FullLog);
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
                        var lastWalletPrice = item.Results.Last().Wallet;
                        var lastWalletInvesting = item.Results.Last().TradingWallet;
                        var newWalletPriceStr = $"{lastWalletPrice:C2}".PadRight(9, ' ');
                        var newWalletInvestingPriceStr = $"{lastWalletInvesting:C2}".PadRight(9, ' ');
                        var percDiff = lastWalletInvesting / lastWalletPrice;
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
