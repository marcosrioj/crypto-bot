
using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class TraderTestService : DomainService, ITraderTestService
    {
        private readonly ITraderService _traderService;
        private readonly IBinanceService _binanceService;

        public TraderTestService(
            ITraderService traderService,
            IBinanceService binanceService)
        {
            _traderService = traderService;
            _binanceService = binanceService;
        }

        public async Task<IEnumerable<CompleteRegressionTestOutputDto>> CompleteRegressionTest(
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            int limitOfDataToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var result = new List<CompleteRegressionTestOutputDto>();

            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();
            var strategies = new List<EStrategy>()
                {EStrategy.SimpleMlStrategy1, EStrategy.NormalMlStrategy1, EStrategy.NormalMlStrategy2, EStrategy.SimpleMeanReversionStrategy, EStrategy.SimpleMicrotrendStrategy};

            var logName = DateTime.Now.ToString("complete-regression-test-TraderTestServiceErrors-yyyy-MM-dd-HH-mm-ss-K");

            foreach (var currency in allCurrencies)
            {
                var data = GetRegressionDataTest(currency, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest, startTime, endTime);

                foreach (var strategy in strategies)
                {
                    try
                    {
                        var regressionTestResult = await RegressionTest(strategy, data, ELogLevel.NoLog);

                        result.Add(new CompleteRegressionTestOutputDto
                        {
                            Currency = currency,
                            Strategy = strategy,
                            Results = regressionTestResult
                        });
                    }
                    catch (Exception e)
                    {
                        LogHelper.Log($"Currency: {currency} - Strategy: {strategy} - Message: {e.Message}", logName);
                    }
                }
            }

            return result;
        }

        public async Task<List<RegressionTestOutputDto>> RegressionTest(
            EStrategy strategy,
            RegressionTestDataOutput data,
            ELogLevel logLevel = ELogLevel.NoLog)
        {
            var newData = data.Clone();

            //Prepare the first stock to test
            var fisrtStockToTest = newData.DataToTest.First();
            newData.DataToTest.Remove(fisrtStockToTest);

            if (logLevel == ELogLevel.FullLog || logLevel == ELogLevel.MainLog)
            {
                LogHelper.Log($"\nRegressionTest - Coin: {newData.Currency}, InitialWallet: {newData.InitialWallet:C2}, Interval: {newData.Interval}, Strategy: {strategy}, ItemsLearned: {newData.LimitOfDataToLearn} (Requested {newData.LimitOfDataToLearnAndTest}), ItemsTested: {newData.LimitOfDataToTest}", "regression_test");
            }

            var result = new List<RegressionTestOutputDto>();

            await RegressionTestExec(
                1, strategy, newData, fisrtStockToTest, newData.InitialWallet, newData.InitialWallet, logLevel, result);

            if (logLevel == ELogLevel.FullLog)
            {
                var i = 1;
                var success = 0m;
                var failed = 0m;
                var message = new StringBuilder();
                foreach (var item in result)
                {
                    if (item.FuturePercDiff > 0 && item.WhatToDo == EWhatToDo.Buy
                        || item.FuturePercDiff <= 0 && item.WhatToDo != EWhatToDo.Buy)
                    {
                        ++success;
                    }
                    else
                    {
                        ++failed;
                    }

                    message.AppendLine(LogHelper.CreateRegressionItemMessage(i, item.FutureStock, item.TradingWallet, item.Wallet, item.WhatToDo, item.FuturePercDiff, item.ActualStock));
                    ++i;
                }

                var successResult = failed != 0 && success != 0 ? success / (success + failed) : 0;
                var failedResult = failed != 0 && success != 0 ? failed / (success + failed) : 0;

                message.AppendLine($"\nRegressionTest Finished - Success: {successResult:P2}({success})- Failed: {failedResult:P2}({failed})");

                LogHelper.Log(message.ToString(), "regression_test");
            }

            return result;
        }

        public RegressionTestDataOutput GetRegressionDataTest(
            ECurrency currency,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 240,
            int limitOfDataToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var sampleStock = _binanceService.GetKline($"{currency}{CryptoBotConsts.BaseCoinName}");
            var dataToLearnAndTest = _binanceService.GetData(currency, interval, startTime, endTime, limitOfDataToLearnAndTest);
            var limitOfDataToLearn = dataToLearnAndTest.Count - limitOfDataToTest;
            var dataToLearn = dataToLearnAndTest.Take(limitOfDataToLearn).ToList();
            var dataToTest = dataToLearnAndTest.Skip(limitOfDataToLearn).Take(limitOfDataToTest).ToList();

            return new RegressionTestDataOutput
            {
                Currency = currency,
                DataToLearn = dataToLearn,
                DataToTest = dataToTest,
                SampleStockToTest = sampleStock,
                Interval = interval,
                LimitOfDataToLearn = limitOfDataToLearn,
                LimitOfDataToLearnAndTest = limitOfDataToLearnAndTest,
                LimitOfDataToTest = limitOfDataToTest,
                InitialWallet = initialWallet
            };
        }

        public async Task<IEnumerable<BetterCoinsToTraderRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();

            var results = new List<BetterCoinsToTraderRightNowOutputDto>();

            foreach (var currency in allCurrencies)
            {
                var data = GetRegressionDataTest(currency, interval, initialWallet, limitOfDataToLearnAndTest, 1, startTime, endTime);

                var regressionTestResult = await RegressionTest(strategy, data, ELogLevel.NoLog);
                var firstRegressionTestResult = regressionTestResult.First();

                results.Add(new BetterCoinsToTraderRightNowOutputDto
                {
                    Currency = currency,
                    ActualStock = firstRegressionTestResult.ActualStock,
                    FuturePercDiff = firstRegressionTestResult.FuturePercDiff,
                    FutureStock = firstRegressionTestResult.FutureStock,
                    TradingWallet = firstRegressionTestResult.TradingWallet,
                    Wallet = firstRegressionTestResult.Wallet,
                    WhatToDo = firstRegressionTestResult.WhatToDo
                });
            }

            return results.Where(x => x.WhatToDo == EWhatToDo.Buy).ToList();
        }

        private async Task RegressionTestExec(
            int index,
            EStrategy strategy,
            RegressionTestDataOutput data,
            IBinanceKline futureStock,
            decimal walletPrice,
            decimal tradingWalletPrice,
            ELogLevel logLevel,
            List<RegressionTestOutputDto> result)
        {
            var resultTraderService = await _traderService.WhatToDo(strategy, data);

            var actualStock = data.DataToLearn.Last();

            var percFuturuValueDiff = (futureStock.Close / actualStock.Close) - 1;

            var newWalletPrice = walletPrice * (percFuturuValueDiff + 1);

            var newTradingWalletPrice = resultTraderService == EWhatToDo.Buy ? tradingWalletPrice * (percFuturuValueDiff + 1) : tradingWalletPrice;

            result.Add(new RegressionTestOutputDto
            {
                ActualStock = actualStock,
                FutureStock = futureStock,
                TradingWallet = newTradingWalletPrice,
                Wallet = newWalletPrice,
                WhatToDo = resultTraderService,
                FuturePercDiff = percFuturuValueDiff
            });

            if (data.DataToTest.Count > 0)
            {
                var nextStockToTest = data.DataToTest.First();
                data.DataToTest.Remove(nextStockToTest);

                var firstValue = data.DataToLearn.First();
                data.DataToLearn.Remove(firstValue);
                data.DataToLearn.Add(futureStock);

                await RegressionTestExec(
                    ++index, strategy, data, nextStockToTest, newWalletPrice, newTradingWalletPrice, logLevel, result);
            }
        }
    }
}
