using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
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

        public TraderService(
            IBinanceService binanceService)
        {
            _binanceService = binanceService;
        }

        public async Task<WhatToDoOutput> WhatToDo(
            RegressionDataOutput data)
        {
            return await Task.FromResult(new WhatToDoOutput());
        }

        public async Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
            RegressionDataOutput data)
        {
            switch (strategy)
            {
                case EStrategy.NormalMlStrategy1:
                    if (data.SampleStockToTest == null)
                    {
                        throw new Exception("NormalMlStrategy must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy1(data);

                case EStrategy.NormalMlStrategy2:
                    if (data.SampleStockToTest == null)
                    {
                        throw new Exception("NormalMlStrategy2 must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy2(data);

                case EStrategy.SimpleMlStrategy1:
                    return await WhatToDoBySimpleMlStrategy1(data);

                case EStrategy.SimpleMeanReversionStrategy:
                    return await WhatToDoBySimpleMeanReversionStrategy(data);

                case EStrategy.SimpleMicrotrendStrategy:
                    return await WhatToDoBySimpleMicrotrendStrategy(data);

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
            var dataToLearn = dataToLearnAndTest.Take(limitOfDataToLearn).ToList();
            var dataToTest = dataToLearnAndTest.Skip(limitOfDataToLearn).Take(limitOfDataToTest).ToList();

            return new RegressionDataOutput
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

        public async Task<List<RegressionOutputDto>> RegressionExec(
            EStrategy strategy,
            RegressionDataOutput data,
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

            var result = new List<RegressionOutputDto>();

            await RegressionItemExec(
                1, strategy, newData, fisrtStockToTest, newData.InitialWallet, newData.InitialWallet, logLevel, result);

            if (logLevel == ELogLevel.FullLog)
            {
                var i = 1;
                var success = 0m;
                var failed = 0m;
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
                var data = GetRegressionData(currency, interval, initialWallet, limitOfDataToLearnAndTest, 1, startTime, endTime);

                var regressionTestResult = await RegressionExec(strategy, data, ELogLevel.NoLog);
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

            return results.Where(x => x.WhatToDo.WhatToDo == EWhatToDo.Buy).ToList();
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMeanReversionStrategy(RegressionDataOutput data)
        {
            var strategy = new MeanReversionStrategy();
            var result = await strategy.ShouldBuyStock(data.DataToLearn);

            if (result.Buy.HasValue)
            {
                if (result.Buy.Value)
                {
                    return await Task.FromResult(new WhatToDoOutput
                    {
                        WhatToDo = EWhatToDo.Buy
                    });
                }
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy
            });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMicrotrendStrategy(RegressionDataOutput data)
        {
            var strategy = new MicrotrendStrategy();
            var result = await strategy.ShouldBuyStock(data.DataToLearn);

            if (result.Buy.HasValue)
            {
                if (result.Buy.Value)
                {
                    return await Task.FromResult(new WhatToDoOutput
                    {
                        WhatToDo = EWhatToDo.Buy
                    });
                }
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy
            });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMlStrategy1(RegressionDataOutput data)
        {
            var strategy = new Strategies.Simple.MLStrategy1.MLStrategy1();
            var result = await strategy.ShouldBuyStock(data.DataToLearn);

            if (result.Buy.HasValue)
            {
                if (result.Buy.Value)
                {
                    return await Task.FromResult(new WhatToDoOutput
                    {
                        WhatToDo = EWhatToDo.Buy,
                        Score = result.Score
                    });
                }
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy
            });
        }

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy1(RegressionDataOutput data)
        {
            var strategy = new Strategies.Normal.MLStrategy1.MLStrategy1();

            var result = await strategy.ShouldBuyStock(data.DataToLearn, data.SampleStockToTest);

            if (result.Buy.HasValue)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy
            });
        }

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy2(RegressionDataOutput data)
        {
            var strategy = new MLStrategy2();

            var result = await strategy.ShouldBuyStock(data.DataToLearn, data.SampleStockToTest);

            if (result.Buy.HasValue)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy
            });
        }

        private async Task RegressionItemExec(
            int index,
            EStrategy strategy,
            RegressionDataOutput data,
            IBinanceKline futureStock,
            decimal walletPrice,
            decimal tradingWalletPrice,
            ELogLevel logLevel,
            List<RegressionOutputDto> result)
        {
            var resultTraderService = await WhatToDo(strategy, data);

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
                    ++index, strategy, data, nextStockToTest, newWalletPrice, newTradingWalletPrice, logLevel, result);
            }
        }
    }
}
