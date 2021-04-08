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
using System.Text;

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
            await Test3();
        }

        private async Task Test3()
        {
            try
            {
                var initialWallet = 1000;
                var interval = KlineInterval.FifteenMinutes;
                var limitOfDataToLearnAndTest = 1000;
                var strategy = EStrategy.NormalMlStrategy2;

                DateTime start = DateTime.UtcNow;
                var result = await _traderTestService.GetBetterCoinsToTraderRightNowAsync(strategy, interval, initialWallet, limitOfDataToLearnAndTest);
                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;
                var seconds = timeDiff.TotalSeconds;

                var success = 0m;
                var failed = 0m;
                var finalResult = new StringBuilder();
                var percTotal = 0m;
                var walletTotal = 0m;
                var tradingWalletTotal = 0m;

                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss K").PadLeft(21, ' ');
                finalResult.AppendLine($"GetBetterCoinsToTraderRightNow: Date: {date} - Strategy: {strategy} - Interval: {interval} - ItemsLearned: {limitOfDataToLearnAndTest}\n");

                result = result.OrderByDescending(x => x.WhatToDo.Score);

                foreach (var item in result)
                {
                    if (item.FuturePercDiff > 0 && item.WhatToDo.WhatToDo == EWhatToDo.Buy
                        || item.FuturePercDiff <= 0 && item.WhatToDo.WhatToDo != EWhatToDo.Buy)
                    {
                        ++success;
                    }
                    else
                    {
                        ++failed;
                    }

                    var percFuturuValueDiffStr = $"{item.FuturePercDiff:P2}".PadLeft(7, ' ');

                    finalResult.AppendLine($"Currency: {item.Currency.ToString().PadLeft(5, ' ')}, FuturePercDiff: {percFuturuValueDiffStr}, Score: {item.WhatToDo.Score}");

                    percTotal = percTotal + item.FuturePercDiff;
                    walletTotal = walletTotal + item.Wallet;
                    tradingWalletTotal = tradingWalletTotal + item.TradingWallet;
                }
                var successResult = failed != 0 && success != 0 ? success / (success + failed) : 0;
                var failedResult = failed != 0 && success != 0 ? failed / (success + failed) : 0;

                var percTotalStr = $"{percTotal/result.Count():P2}".PadLeft(7, ' ');
                var totalWalletStr = $"{walletTotal:C2}".PadLeft(10, ' ');
                var totalTradindWalletStr = $"{tradingWalletTotal:C2}".PadLeft(10, ' ');
                var initialWalletStr = $"{result.Count() * initialWallet:C2}".PadLeft(10, ' ');

                finalResult.AppendLine($"PercEarn: {percTotalStr}, InitialWallet: {initialWalletStr}, FinalWallet: {totalWalletStr}, FinalTradingWallet: {totalTradindWalletStr}");
                finalResult.AppendLine($"Success: {successResult:P2}({success})- Failed: {failedResult:P2}({failed})");
                finalResult.AppendLine($"TimeExecution: {seconds} seconds");

                LogHelper.Log(finalResult.ToString(), "get-better-coins-to-trader-right-now");

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

                var data = _traderTestService.GetRegressionDataTest(ECurrency.DENT, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderTestService.RegressionTest(EStrategy.SimpleMlStrategy1, data, ELogLevel.FullLog);

                data = _traderTestService.GetRegressionDataTest(ECurrency.BTC, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderTestService.RegressionTest(EStrategy.SimpleMlStrategy1, data, ELogLevel.FullLog);

                data = _traderTestService.GetRegressionDataTest(ECurrency.XRP, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderTestService.RegressionTest(EStrategy.SimpleMlStrategy1, data, ELogLevel.FullLog);

                data = _traderTestService.GetRegressionDataTest(ECurrency.EOS, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                await _traderTestService.RegressionTest(EStrategy.SimpleMlStrategy1, data, ELogLevel.FullLog);
                //await _traderTestService.RegressionTest(EStrategy.NormalMlStrategy1, data, ELogLevel.FullLog);
                //await _traderTestService.RegressionTest(EStrategy.NormalMlStrategy2, data, ELogLevel.FullLog);
                //await _traderTestService.RegressionTest(EStrategy.SimpleMicrotrendStrategy, data, ELogLevel.FullLog);
                //await _traderTestService.RegressionTest(EStrategy.SimpleMeanReversionStrategy, data, ELogLevel.FullLog);
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
