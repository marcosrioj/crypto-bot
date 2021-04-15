using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy1;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy2;
using CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy;
using CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class TraderService : DomainService, ITraderService
    {
        private readonly IBinanceService _binanceService;
        private readonly IMLStrategy1 _normalMlStrategy1;
        private readonly IMLStrategy2 _normalMlStrategy2;
        private readonly IMeanReversionStrategy _simpleMeanReversionStrategy;
        private readonly IMicrotrendStrategy _simpleMicrotrendStrategy;
        private readonly Strategies.Simple.MLStrategy1.IMLStrategy1 _simpleMlStrategy1;

        public TraderService(
            IBinanceService binanceService,
            IMLStrategy1 normalMlStrategy1,
            IMLStrategy2 normalMlStrategy2,
            IMeanReversionStrategy simpleMeanReversionStrategy,
            IMicrotrendStrategy simpleMicrotrendStrategy,
            Strategies.Simple.MLStrategy1.IMLStrategy1 simpleMlStrategy1)
        {
            _binanceService = binanceService;
            _normalMlStrategy1 = normalMlStrategy1;
            _normalMlStrategy2 = normalMlStrategy2;
            _simpleMeanReversionStrategy = simpleMeanReversionStrategy;
            _simpleMicrotrendStrategy = simpleMicrotrendStrategy;
            _simpleMlStrategy1 = simpleMlStrategy1;

        }

        public async Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionDataOutput data)
        {
            switch (strategy)
            {
                case EStrategy.NormalMlStrategy1:
                    if (data.SampleStockToTest == null)
                    {
                        throw new Exception("NormalMlStrategy must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy1(data, eInvestorProfile);

                case EStrategy.NormalMlStrategy2:
                    if (data.SampleStockToTest == null)
                    {
                        throw new Exception("NormalMlStrategy2 must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy2(data, eInvestorProfile);

                case EStrategy.SimpleMlStrategy1:
                    return await WhatToDoBySimpleMlStrategy1(data, eInvestorProfile);

                case EStrategy.SimpleMeanReversionStrategy:
                    return await WhatToDoBySimpleMeanReversionStrategy(data, eInvestorProfile);

                case EStrategy.SimpleMicrotrendStrategy:
                    return await WhatToDoBySimpleMicrotrendStrategy(data, eInvestorProfile);

                default:
                    throw new Exception("Strategy not found");
            }
        }

        public RegressionDataOutput GetRegressionData(
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

            return new RegressionDataOutput
            {
                Currency = currency,
                DataToLearnAndTest = dataToLearnAndTest,
                SampleStockToTest = sampleStock,
                Interval = interval,
                LimitOfDataToLearn = limitOfDataToLearn,
                LimitOfDataToLearnAndTest = limitOfDataToLearnAndTest,
                LimitOfDataToTest = limitOfDataToTest,
                InitialWallet = initialWallet
            };
        }

        public async Task<List<RegressionOutputDto>> RegressionExec(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionDataOutput data,
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

            var result = new List<RegressionOutputDto>();

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
                        else {
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

        public async Task<List<BetterCoinsToTraderRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
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
                var data = GetRegressionData(currency, interval, initialWallet, limitOfDataToLearnAndTest, 1, startTime, endTime);

                var regressionTestResult = await RegressionExec(strategy, eInvestorProfile, data, ELogLevel.NoLog);
                var firstRegressionTestResult = regressionTestResult.First();

                results.Add(new BetterCoinsToTraderRightNowOutputDto
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

        public async Task<List<BetterCoinsToTraderRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
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

            var result = await GetBetterCoinsToTraderRightNowAsync(firstStrategy, eInvestorProfile, interval, initialWallet, limitOfDataToLearnAndTest);

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

        public async Task<List<BetterCoinsToTraderRightNowOutputDto>> FilterBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            List<BetterCoinsToTraderRightNowOutputDto> input)
        {
            var result = new List<BetterCoinsToTraderRightNowOutputDto>();

            foreach (var item in input)
            {
                var regressionTestResult = await RegressionExec(strategy, eInvestorProfile, item.Data, ELogLevel.NoLog);
                var firstRegressionTestResult = regressionTestResult.First();

                if (firstRegressionTestResult.WhatToDo.WhatToDo == EWhatToDo.Buy)
                {
                    result.Add(new BetterCoinsToTraderRightNowOutputDto
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

        private async Task<WhatToDoOutput> WhatToDoBySimpleMeanReversionStrategy(RegressionDataOutput data, EInvestorProfile eInvestorProfile)
        {
            var result = await _simpleMeanReversionStrategy.ShouldBuyStock(data.DataToLearn, eInvestorProfile);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score
            });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMicrotrendStrategy(RegressionDataOutput data, EInvestorProfile eInvestorProfile)
        {
            var result = await _simpleMicrotrendStrategy.ShouldBuyStock(data.DataToLearn, eInvestorProfile);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score
            });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMlStrategy1(RegressionDataOutput data, EInvestorProfile eInvestorProfile)
        {
            var result = await _simpleMlStrategy1.ShouldBuyStock(data.DataToLearn, eInvestorProfile);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score
            });
        }

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy1(RegressionDataOutput data, EInvestorProfile eInvestorProfile)
        {
            var result = await _normalMlStrategy1.ShouldBuyStock(data.DataToLearn, eInvestorProfile, data.SampleStockToTest);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score
            });
        }

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy2(RegressionDataOutput data, EInvestorProfile eInvestorProfile)
        {
            var result = await _normalMlStrategy2.ShouldBuyStock(data.DataToLearn, eInvestorProfile, data.SampleStockToTest);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score
            });
        }

        private async Task RegressionItemExec(
            int index,
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionDataOutput data,
            IBinanceKline futureStock,
            decimal walletPrice,
            decimal tradingWalletPrice,
            ELogLevel logLevel,
            List<RegressionOutputDto> result)
        {
            var resultTraderService = await WhatToDo(strategy, eInvestorProfile, data);

            var actualStock = data.DataToLearn.Last();

            var percFuturuValueDiff = (futureStock.Close / actualStock.Close) - 1;

            var newWalletPrice = walletPrice * (percFuturuValueDiff + 1);

            var newTradingWalletPrice = resultTraderService.WhatToDo == EWhatToDo.Buy ? tradingWalletPrice * (percFuturuValueDiff + 1) : tradingWalletPrice;

            result.Add(new RegressionOutputDto
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
