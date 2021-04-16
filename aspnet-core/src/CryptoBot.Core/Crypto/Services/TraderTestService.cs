using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class TraderTestService : DomainService, ITraderTestService
    {
        public readonly IRepository<Trading, long> _tradingRepository;

        private readonly IWalletService _walletService;
        private readonly ITraderService _traderService;
        private readonly IBinanceService _binanceService;

        public TraderTestService(
            IRepository<Trading, long> tradingRepository,
            IWalletService walletService,
            ITraderService traderService,
            IBinanceService binanceService)
        {
            _tradingRepository = tradingRepository;
            _walletService = walletService;
            _traderService = traderService;
            _binanceService = binanceService;
        }

        public async Task<IEnumerable<CompleteRegressionTestOutputDto>> CompleteRegressionTest(
            EInvestorProfile investorProfile,
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
                var data = GetRegressionData(currency, interval, initialWallet, limitOfDataToLearnAndTest, limitOfDataToTest, startTime, endTime);

                foreach (var strategy in strategies)
                {
                    try
                    {
                        var regressionTestResult = await RegressionExec(strategy, investorProfile, data, ELogLevel.NoLog);

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


        public RegressionTestDataOutput GetRegressionData(
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

            return new RegressionTestDataOutput
            {
                Currency = currency,
                DataToLearnAndTest = dataToLearnAndTest,
                StockRightNow = sampleStock,
                Interval = interval,
                LimitOfDataToLearn = limitOfDataToLearn,
                LimitOfDataToLearnAndTest = limitOfDataToLearnAndTest,
                LimitOfDataToTest = limitOfDataToTest,
                InitialWallet = initialWallet
            };
        }

        public async Task<List<RegressionTestOutputDto>> RegressionExec(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionTestDataOutput data,
            ELogLevel logLevel = ELogLevel.NoLog)
        {
            var newData = data.Clone();

            //Prepare the first stock to test//TODO
            var fisrtStockToTest = newData.DataToTest.First();
            newData.DataToTest.Remove(fisrtStockToTest);

            if (logLevel == ELogLevel.FullLog || logLevel == ELogLevel.MainLog)
            {
                LogHelper.Log($"\nRegressionTest - Coin: {newData.Currency}, InitialWallet: {newData.InitialWallet:C2}, Interval: {newData.Interval}, Strategy: {strategy}, InvestorProfile: {eInvestorProfile}, ItemsLearned: {newData.LimitOfDataToLearn} (Requested {newData.LimitOfDataToLearnAndTest}), ItemsTested: {newData.LimitOfDataToTest}", "regression_test");
            }

            var result = new List<RegressionTestOutputDto>();

            await RegressionItemExec(
                1, strategy, eInvestorProfile, newData, fisrtStockToTest, newData.InitialWallet, newData.InitialWallet, logLevel, result);

            if (logLevel == ELogLevel.FullLog)
            {
                var i = 1;
                var success = 0m;
                var successBuy = 0m;
                var failed = 0m;
                var failedBuy = 0m;
                var message = new StringBuilder();
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

                    if (item.WhatToDo.WhatToDo == EWhatToDo.Buy)
                    {
                        if (item.FuturePercDiff > 0)
                        {
                            ++successBuy;
                        }
                        else
                        {
                            ++failedBuy;
                        }

                    }

                    message.AppendLine(LogHelper.CreateRegressionItemMessage(i, item.FutureStock, item.TradingWallet, item.Wallet, item.WhatToDo, item.FuturePercDiff, item.ActualStock));
                    ++i;
                }

                var successResult = failed != 0 && success != 0 ? success / (success + failed) : 0;
                var successBuyResult = failedBuy != 0 && successBuy != 0 ? successBuy / (successBuy + failedBuy) : 0;
                var failedResult = failed != 0 && success != 0 ? failed / (success + failed) : 0;
                var failedBuyResult = failedBuy != 0 && successBuy != 0 ? failedBuy / (successBuy + failedBuy) : 0;

                message.AppendLine($"\nRegressionTest Finished - SuccessBuy: {successBuyResult:P2}({successBuy})- FailedBuy: {failedBuyResult:P2}({failedBuy}) --- Success: {successResult:P2}({success})- Failed: {failedResult:P2}({failed})");

                LogHelper.Log(message.ToString(), "regression_test");
            }

            return result;
        }

        public async Task<List<BetterCoinsToTestTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();

            var results = new List<BetterCoinsToTestTradeRightNowOutputDto>();

            foreach (var currency in allCurrencies)
            {
                if (currency == ECurrency.USDT)
                    continue;

                var data = GetRegressionData(currency, interval, initialWallet, limitOfDataToLearnAndTest, 1, startTime, endTime);

                var regressionTestResult = await RegressionExec(strategy, eInvestorProfile, data, ELogLevel.NoLog);
                var firstRegressionTestResult = regressionTestResult.First();

                results.Add(new BetterCoinsToTestTradeRightNowOutputDto
                {
                    Currency = currency,
                    ActualStock = firstRegressionTestResult.ActualStock,
                    FuturePercDiff = firstRegressionTestResult.FuturePercDiff,
                    FutureStock = firstRegressionTestResult.FutureStock,
                    TradingWallet = firstRegressionTestResult.TradingWallet,
                    Wallet = firstRegressionTestResult.Wallet,
                    WhatToDo = firstRegressionTestResult.WhatToDo,
                    Data = data
                });
            }

            return results.Where(x => x.WhatToDo.WhatToDo == EWhatToDo.Buy).ToList();
        }

        public async Task<List<BetterCoinsToTestTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            List<EStrategy> strategies,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            ELogLevel logLevel = ELogLevel.NoLog,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            if (!strategies.Any())
            {
                throw new ArgumentException("Must to have at least one strategy");
            }

            var messageLogger = new StringBuilder();
            DateTime start = DateTime.UtcNow;

            var firstStrategy = strategies.First();
            strategies.Remove(firstStrategy);

            var result = await GetBetterCoinsToTraderRightNowAsync(firstStrategy, eInvestorProfile, interval, initialWallet, limitOfDataToLearnAndTest, startTime, endTime);

            if (logLevel == ELogLevel.FullLog || logLevel == ELogLevel.FullLog)
            {
                messageLogger.AppendLine(LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, firstStrategy, eInvestorProfile, result).ToString());
            }

            foreach (var strategy in strategies)
            {
                result = await FilterBetterCoinsToTraderRightNowAsync(strategy, eInvestorProfile, result);

                if (logLevel == ELogLevel.FullLog || logLevel == ELogLevel.FullLog)
                {
                    messageLogger.AppendLine(LogHelper.CreateBetterCoinsToTraderRightNowMessage(initialWallet, interval, limitOfDataToLearnAndTest, strategy, eInvestorProfile, result).ToString());
                }
            }

            if (logLevel == ELogLevel.FullLog || logLevel == ELogLevel.FullLog)
            {
                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;
                var seconds = timeDiff.TotalSeconds;

                messageLogger.AppendLine($"\nTimeExecution: {seconds} seconds");

                LogHelper.Log($"{messageLogger}\n", "get-better-coins-to-trader-right-now");
            }

            return result;
        }

        public async Task<List<BetterCoinsToTestTradeRightNowOutputDto>> FilterBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            List<BetterCoinsToTestTradeRightNowOutputDto> input)
        {
            var result = new List<BetterCoinsToTestTradeRightNowOutputDto>();

            foreach (var item in input)
            {
                var regressionTestResult = await RegressionExec(strategy, eInvestorProfile, item.Data, ELogLevel.NoLog);
                var firstRegressionTestResult = regressionTestResult.First();

                if (firstRegressionTestResult.WhatToDo.WhatToDo == EWhatToDo.Buy)
                {
                    result.Add(new BetterCoinsToTestTradeRightNowOutputDto
                    {
                        Currency = item.Currency,
                        ActualStock = firstRegressionTestResult.ActualStock,
                        FuturePercDiff = firstRegressionTestResult.FuturePercDiff,
                        FutureStock = firstRegressionTestResult.FutureStock,
                        TradingWallet = firstRegressionTestResult.TradingWallet,
                        Wallet = firstRegressionTestResult.Wallet,
                        WhatToDo = firstRegressionTestResult.WhatToDo,
                        Data = item.Data
                    });
                }
            }

            return result;
        }

        public async Task AutoTraderWithWalletVirtualAsync(long userId)
        {
            var mainWallet = await _walletService.GetOrCreate(ECurrency.USDT, EWalletType.Virtual, userId, 1000);
            var tragindId = await CreateTrading(mainWallet.Id, mainWallet.Balance);

            var interval = KlineInterval.FiveMinutes;
            var limitOfDataToLearnAndTest = 1000;
            var strategies = new List<EStrategy>() {
                    EStrategy.SimpleMeanReversionStrategy
                };
            var investorProfile = EInvestorProfile.UltraConservative;

            var betterCoinsToTrade = await GetBetterCoinsToTraderRightNowAsync(strategies, investorProfile, interval, mainWallet.Balance, limitOfDataToLearnAndTest, ELogLevel.NoLog);


        }

        private async Task<long> CreateTrading(long walletId, decimal startbalance)
        {
            var tradingId = await _tradingRepository.InsertAndGetIdAsync(new Trading
            {
                StartBalance = startbalance,
                WalletId = walletId
            });

            await CurrentUnitOfWork.SaveChangesAsync();

            return tradingId;
        }

        private async Task UpdateTrading(long id, decimal balance)
        {
            var trading = await _tradingRepository
                .GetAll()
                .Where(x => x.Id == id)
                .FirstAsync();

            trading.EndBalance = balance;

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task RegressionItemExec(
            int index,
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionTestDataOutput data,
            IBinanceKline futureStock,
            decimal walletPrice,
            decimal tradingWalletPrice,
            ELogLevel logLevel,
            List<RegressionTestOutputDto> result)
        {
            var resultTraderService = await _traderService.WhatToDo(strategy, eInvestorProfile, data);

            var actualStock = data.DataToLearn.Last();

            var percFuturuValueDiff = (futureStock.Close / actualStock.Close) - 1;

            var newWalletPrice = walletPrice * (percFuturuValueDiff + 1);

            var newTradingWalletPrice = resultTraderService.WhatToDo == EWhatToDo.Buy ? tradingWalletPrice * (percFuturuValueDiff + 1) : tradingWalletPrice;

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

                await RegressionItemExec(
                    ++index, strategy, eInvestorProfile, data, nextStockToTest, newWalletPrice, newTradingWalletPrice, logLevel, result);
            }
        }
    }
}
