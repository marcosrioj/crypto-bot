
using Abp.Domain.Services;
using Binance.Net.Enums;

using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Strategies;


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

        public async Task RegressionTest(
            ECurrency currency,
            KlineInterval interval,
            int limitOfDetailsToLearnAndTest = 1000,
            int limitOfDetailsToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {

        }

        public async Task RegressionTest(
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
            var limitOfDetailsToLearn = limitOfDetailsToLearnAndTest - limitOfDetailsToTest;

            var dataToLearnAndTest = _binanceService.GetData(currency, interval, startTime, endTime, limitOfDetailsToLearnAndTest);

            var dataToLearn = dataToLearnAndTest.Take(limitOfDetailsToLearn).ToList();
            var dataToTest = dataToLearnAndTest.Skip(limitOfDetailsToLearn).Take(limitOfDetailsToTest).ToList();

            _traderService.SetData(currency, dataToLearn);

            //Prepare the first stock to test
            var fisrtStockToTest = dataToTest.First();
            dataToTest.Remove(fisrtStockToTest);

            if (logLevel == ELogLevel.FullLog || logLevel == ELogLevel.MainLog)
            {
                LogHelper.Log($"\nRegressionTest - Coin: {currency}, InitialWallet: {initialWallet:C2}, InitialWalletInvesting: {initialWallet:C2}, Interval: {interval}", "regression_test");
            }

            await RegressionTestExec(
                1, strategy, currency, dataToLearn, dataToTest, fisrtStockToTest, initialWallet, initialWallet, logLevel);
        }

        private async Task RegressionTestExec(
            int index,
            EStrategy strategy,
            ECurrency currency,
            List<IBinanceKline> dataToLearn,
            List<IBinanceKline> dataToTest,
            IBinanceKline futureStock,
            decimal walletPrice,
            decimal walletInvestingPrice,
            ELogLevel logLevel)
        {
            var result = await _traderService.WhatToDo(strategy, currency);

            var actualStock = dataToLearn.Last();

            var percFuturuValueDiff = (futureStock.Close / actualStock.Close) - 1;

            var newWalletPrice = walletPrice * (percFuturuValueDiff + 1);

            var newWalletInvestingPrice = result == EWhatToDo.Buy ? walletInvestingPrice * (percFuturuValueDiff + 1) : walletInvestingPrice;

            if (logLevel == ELogLevel.FullLog)
            {
                LogHelper.LogRegressionItemTest(index, futureStock, newWalletInvestingPrice, newWalletPrice, result, percFuturuValueDiff, actualStock);
            }

            if (dataToTest.Count > 0)
            {
                var nextStockToTest = dataToTest.First();
                dataToTest.Remove(nextStockToTest);

                var firstValue = dataToLearn.First();
                dataToLearn.Remove(firstValue);
                dataToLearn.Add(futureStock);

                await RegressionTestExec(
                    ++index, strategy, currency, dataToLearn, dataToTest, nextStockToTest, newWalletPrice, newWalletInvestingPrice, logLevel);
            }

        }
    }
}
