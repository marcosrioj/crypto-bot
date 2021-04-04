
using Abp.Domain.Services;
using Binance.Net.Enums;

using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
using JetBrains.Annotations;


namespace CryptoBot.Crypto.Services
{
    public class TraderTestService : DomainService, ITraderTestService
    {
        private readonly ITraderService _traderService;
        private readonly IBinanceService _binanceService;

        private IBinanceKline _sampleStock;

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
            int limitOfDetailsToLearnAndTest = 1000,
            int limitOfDetailsToTest = 120,
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
                SetSampleStock(currency);

                foreach (var strategy in strategies)
                {
                    try
                    {
                        var regressionTestResult = await RegressionTest(
                            strategy, currency, interval, initialWallet,
                            ELogLevel.NoLog, limitOfDetailsToLearnAndTest,
                            limitOfDetailsToTest, startTime, endTime);

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
            ECurrency currency,
            KlineInterval interval,
            decimal initialWallet,
            ELogLevel logLevel = ELogLevel.NoLog,
            int limitOfDetailsToLearnAndTest = 240,
            int limitOfDetailsToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            if (_sampleStock == null)
            {
                SetSampleStock(currency);
            }

            var dataToLearnAndTest = _binanceService.GetData(currency, interval, startTime, endTime, limitOfDetailsToLearnAndTest);

            var limitOfDetailsToLearn = dataToLearnAndTest.Count - limitOfDetailsToTest;
            var dataToLearn = dataToLearnAndTest.Take(limitOfDetailsToLearn).ToList();
            var dataToTest = dataToLearnAndTest.Skip(limitOfDetailsToLearn).Take(limitOfDetailsToTest).ToList();

            _traderService.SetData(currency, dataToLearn);

            //Prepare the first stock to test
            var fisrtStockToTest = dataToTest.First();
            dataToTest.Remove(fisrtStockToTest);

            if (logLevel == ELogLevel.FullLog || logLevel == ELogLevel.MainLog)
            {
                LogHelper.Log($"\nRegressionTest - Coin: {currency}, InitialWallet: {initialWallet:C2}, Interval: {interval}, Strategy: {strategy}, ItemsLearned: {limitOfDetailsToLearn} (Requested {limitOfDetailsToLearnAndTest}), ItemsTested: {limitOfDetailsToTest}", "regression_test");
            }

            var result = new List<RegressionTestOutputDto>();

            await RegressionTestExec(
                1, strategy, currency, dataToLearn, dataToTest, fisrtStockToTest, initialWallet, initialWallet, logLevel, result);

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

        private async Task RegressionTestExec(
            int index,
            EStrategy strategy,
            ECurrency currency,
            List<IBinanceKline> dataToLearn,
            List<IBinanceKline> dataToTest,
            IBinanceKline futureStock,
            decimal walletPrice,
            decimal tradingWalletPrice,
            ELogLevel logLevel,
            List<RegressionTestOutputDto> result)
        {
            var resultTraderService = await _traderService.WhatToDo(strategy, currency, _sampleStock);

            var actualStock = dataToLearn.Last();

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

            if (dataToTest.Count > 0)
            {
                var nextStockToTest = dataToTest.First();
                dataToTest.Remove(nextStockToTest);

                var firstValue = dataToLearn.First();
                dataToLearn.Remove(firstValue);
                dataToLearn.Add(futureStock);

                await RegressionTestExec(
                    ++index, strategy, currency, dataToLearn, dataToTest, nextStockToTest, newWalletPrice, newTradingWalletPrice, logLevel, result);
            }
        }

        private void SetSampleStock(ECurrency currency)
        {
            var sampleStock = _binanceService.GetKline($"{currency}{CryptoBotConsts.BaseCoinName}");
            _sampleStock = sampleStock;
        }
    }
}
