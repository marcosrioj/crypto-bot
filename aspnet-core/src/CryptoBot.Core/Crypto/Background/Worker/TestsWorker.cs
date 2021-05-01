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

namespace CryptoBot.Crypto.BackgroundWorker.Worker
{
    public class TestsWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ITraderService _traderService;
        private readonly IBinanceService _binanceService;
        private readonly ITraderTestService _traderTestService;

        public TestsWorker(
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
            Test2();
        }

        private async Task BinanceTest()
        {
            _binanceService.Samples();
        }

        private async Task Test5()
        {
            try
            {
                var initialWallet = 1000;
                var interval = KlineInterval.FifteenMinutes;
                var tradingType = ETradingType.Spot;
                var profitWay = EProfitWay.ProfitFromGain;
                var limitOfDataToLearnAndTest = 1000;
                var limitOfDataToTest = 0;
                var investorProfile = EInvestorProfile.UltraConservative;
                var currency = ECurrency.DOGE;

                var data1 = _traderTestService.GetRegressionData(currency, interval, tradingType, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);

                if (data1 == null)
                {
                    throw new Exception($"No data: {tradingType} {currency}");
                }

                await _traderTestService.RegressionExec(new List<EStrategy>() { EStrategy.SimpleMlStrategy1 }, investorProfile, tradingType, profitWay, data1, ELogLevel.FullLog);

                //var data2 = _traderService.GetRegressionData(ECurrency.BNB, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                //await _traderService.RegressionExec(EStrategy.NormalMlStrategy2, investorProfile, data2, ELogLevel.FullLog);
            }
            catch (Exception e)
            {
                LogHelper.Log($"Worker error: TraderWorker - Message: {e.Message}", $"regression_test-ERROR");
            }
        }

        private async Task Test4()
        {
            try
            {
                var initialWallet = 1000;
                var interval = KlineInterval.FiveMinutes;
                var limitOfDataToLearnAndTest = 1000;
                var tradingType = ETradingType.Futures;
                var profitWay = EProfitWay.ProfitFromLoss;
                var strategies = new List<EStrategy>() {
                    EStrategy.SimpleMeanReversionStrategy,
                    EStrategy.NormalMlStrategy1
                };
                var investorProfile = EInvestorProfile.UltraConservative;

                var result1 = await _traderTestService.GetBetterCoinsToTraderRightNowAsync(strategies, investorProfile, interval, tradingType, profitWay, initialWallet, limitOfDataToLearnAndTest, ELogLevel.FullLog);

                //strategies = new List<EStrategy>() {
                //    EStrategy.SimpleMeanReversionStrategy,
                //    EStrategy.NormalMlStrategy2
                //};
                //var result2 = await _traderTestService.GetBetterCoinsToTraderRightNowAsync(strategies, investorProfile, interval, tradingType, profitWay, initialWallet, limitOfDataToLearnAndTest, ELogLevel.FullLog);

                //strategies = new List<EStrategy>() {
                //    EStrategy.SimpleMeanReversionStrategy,
                //    EStrategy.SimpleMlStrategy1,
                //    EStrategy.NormalMlStrategy2
                //};
                //var result4 = await _traderTestService.GetBetterCoinsToTraderRightNowAsync(strategies, investorProfile, interval, tradingType, profitWay, initialWallet, limitOfDataToLearnAndTest, ELogLevel.FullLog);
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
                var tradingType = ETradingType.Futures;
                var profitWay = EProfitWay.ProfitFromLoss;
                var limitOfDataToLearnAndTest = 1000;
                var strategies = new List<EStrategy>() { EStrategy.SimpleMeanReversionStrategy };
                var investorProfile = EInvestorProfile.UltraConservative;

                DateTime start = DateTime.UtcNow;
                var result = await _traderTestService.GetBetterCoinsToTraderRightNowAsync(strategies, investorProfile, interval, tradingType, profitWay, initialWallet, limitOfDataToLearnAndTest);
                var messageResult1 = LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, strategies, investorProfile, tradingType, profitWay, result);

                var result2 = await _traderTestService.FilterBetterCoinsToTraderRightNowAsync(strategies, investorProfile, tradingType, profitWay, result);
                var messageResult2 = LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, strategies, investorProfile, tradingType, profitWay, result2);

                //var result3 = await _traderTestService.FilterBetterCoinsToTraderRightNowAsync(EStrategy.NormalMlStrategy2, investorProfile, tradingType, profitWay, result2);
                //var messageResult3 = LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, EStrategy.NormalMlStrategy2, investorProfile, tradingType, profitWay, result3);

                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;
                var seconds = timeDiff.TotalSeconds;

                messageResult2.AppendLine($"\nTimeExecution: {seconds} seconds");

                LogHelper.Log($"{messageResult1}\n\n{messageResult2}\n", "get-better-coins-to-trader-right-now");

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
                var interval = KlineInterval.FifteenMinutes;
                var tradingType = ETradingType.Spot;
                var profitWay = EProfitWay.ProfitFromLoss;
                var limitOfDataToLearnAndTest = 10000;
                var limitOfDataToTest = 120;
                var investorProfile = EInvestorProfile.UltraConservative;
                var strategies = new List<EStrategy>() { EStrategy.NormalMlStrategy2 };

                var data = _traderTestService.GetRegressionData(ECurrency.BTC, interval, tradingType, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                if (data == null)
                {
                    throw new Exception($"No data: {tradingType} {ECurrency.BTC}");
                }
                await _traderTestService.RegressionExec(strategies, investorProfile, tradingType, profitWay, data, ELogLevel.FullLog);

                data = _traderTestService.GetRegressionData(ECurrency.ETH, interval, tradingType, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                if (data == null)
                {
                    throw new Exception($"No data: {tradingType} {ECurrency.ETH}");
                }
                await _traderTestService.RegressionExec(strategies, investorProfile, tradingType, profitWay, data, ELogLevel.FullLog);

                data = _traderTestService.GetRegressionData(ECurrency.XRP, interval, tradingType, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                if (data == null)
                {
                    throw new Exception($"No data: {tradingType} {ECurrency.XRP}");
                }
                await _traderTestService.RegressionExec(strategies, investorProfile, tradingType, profitWay, data, ELogLevel.FullLog);

                data = _traderTestService.GetRegressionData(ECurrency.BNB, interval, tradingType, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                if (data == null)
                {
                    throw new Exception($"No data: {tradingType} {ECurrency.BNB}");
                }
                await _traderTestService.RegressionExec(strategies, investorProfile, tradingType, profitWay, data, ELogLevel.FullLog);

                data = _traderTestService.GetRegressionData(ECurrency.HOT, interval, tradingType, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest);
                if (data == null)
                {
                    throw new Exception($"No data: {tradingType} {ECurrency.HOT}");
                }
                await _traderTestService.RegressionExec(strategies, investorProfile, tradingType, profitWay, data, ELogLevel.FullLog);
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
                var tradingType = ETradingType.Spot;
                var profitWay = EProfitWay.ProfitFromGain;
                var interval = KlineInterval.FifteenMinutes;
                var investorProfile = EInvestorProfile.UltraConservative;

                var result = await _traderTestService.CompleteRegressionTest(investorProfile, interval, tradingType, profitWay, initialWallet, limitOfDetailsToLearnAndTest, limitOfDetailsToTest);

                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss K").PadLeft(21, ' ');
                LogHelper.Log($"\n\nDate: {date} - Interval: {interval} - TradingType: {tradingType} -  ItemsLearned: {limitOfDetailsToLearnAndTest - limitOfDetailsToTest} - ItemsTested: {limitOfDetailsToTest}", logName);

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
