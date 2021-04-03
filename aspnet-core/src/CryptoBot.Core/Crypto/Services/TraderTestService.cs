
using Abp.Domain.Services;
using Binance.Net.Enums;

using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;


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
            int limitOfDetailsToLearnAndTest = 1000,
            int limitOfDetailsToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var result = new List<CompleteRegressionTestOutputDto>();

            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();
            var strategies = new List<EStrategy>()
                {EStrategy.SimpleMlStrategy, EStrategy.SimpleMeanReversionStrategy, EStrategy.SimpleMicrotrendStrategy};

            var logName = DateTime.Now.ToString("complete-regression-test-TraderTestServiceErrors-yyyy-MM-dd-HH-mm-ss-K");

            foreach (var currency in allCurrencies)
            {
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
                            FinalTradingWallet = regressionTestResult.FinalTradingWallet,
                            FinalWallet = regressionTestResult.FinalWallet
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

        public async Task<RegressionTestOutputDto> RegressionTest(
            EStrategy strategy,
            ECurrency currency,
            KlineInterval interval,
            decimal initialWallet,
            ELogLevel logLevel = ELogLevel.NoLog,
            int limitOfDetailsToLearnAndTest = 1000,
            int limitOfDetailsToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
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
                LogHelper.Log($"\nRegressionTest - Coin: {currency}, InitialWallet: {initialWallet:C2}, Interval: {interval}, Strategy: {strategy}, ItemsLearned: {limitOfDetailsToLearn} (Requested {limitOfDetailsToLearnAndTest})", "regression_test");
            }

            return await RegressionTestExec(
                1, strategy, currency, dataToLearn, dataToTest, fisrtStockToTest, initialWallet, initialWallet, logLevel);
        }

        private async Task<RegressionTestOutputDto> RegressionTestExec(
            int index,
            EStrategy strategy,
            ECurrency currency,
            List<IBinanceKline> dataToLearn,
            List<IBinanceKline> dataToTest,
            IBinanceKline futureStock,
            decimal walletPrice,
            decimal tradingWalletPrice,
            ELogLevel logLevel)
        {
            var result = await _traderService.WhatToDo(strategy, currency);

            var actualStock = dataToLearn.Last();

            var percFuturuValueDiff = (futureStock.Close / actualStock.Close) - 1;

            var newWalletPrice = walletPrice * (percFuturuValueDiff + 1);

            var newTradingWalletPrice = result == EWhatToDo.Buy ? tradingWalletPrice * (percFuturuValueDiff + 1) : tradingWalletPrice;

            if (logLevel == ELogLevel.FullLog)
            {
                LogHelper.LogRegressionItemTest(index, futureStock, newTradingWalletPrice, newWalletPrice, result, percFuturuValueDiff, actualStock);
            }

            if (dataToTest.Count > 0)
            {
                var nextStockToTest = dataToTest.First();
                dataToTest.Remove(nextStockToTest);

                var firstValue = dataToLearn.First();
                dataToLearn.Remove(firstValue);
                dataToLearn.Add(futureStock);
                
                return await RegressionTestExec(
                    ++index, strategy, currency, dataToLearn, dataToTest, nextStockToTest, newWalletPrice, newTradingWalletPrice, logLevel);
            }

            return new RegressionTestOutputDto
            {
                FinalTradingWallet = tradingWalletPrice,
                FinalWallet = walletPrice
            };
        }
    }
}
