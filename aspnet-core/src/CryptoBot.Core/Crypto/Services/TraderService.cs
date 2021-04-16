using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy1;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy2;
using CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy;
using CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class TraderService : DomainService, ITraderService
    {
        public readonly IRepository<Trading, long> _tradingRepository;

        private readonly IBinanceService _binanceService;
        private readonly IWalletService _walletService;
        private readonly IMLStrategy1 _normalMlStrategy1;
        private readonly IMLStrategy2 _normalMlStrategy2;
        private readonly IMeanReversionStrategy _simpleMeanReversionStrategy;
        private readonly IMicrotrendStrategy _simpleMicrotrendStrategy;
        private readonly Strategies.Simple.MLStrategy1.IMLStrategy1 _simpleMlStrategy1;

        public TraderService(
            IRepository<Trading, long> tradingRepository,
            IBinanceService binanceService,
            IWalletService walletService,
            IMLStrategy1 normalMlStrategy1,
            IMLStrategy2 normalMlStrategy2,
            IMeanReversionStrategy simpleMeanReversionStrategy,
            IMicrotrendStrategy simpleMicrotrendStrategy,
            Strategies.Simple.MLStrategy1.IMLStrategy1 simpleMlStrategy1)
        {
            _tradingRepository = tradingRepository;

            _binanceService = binanceService;
            _walletService = walletService;
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
                    if (data.StockRightNow == null)
                    {
                        throw new Exception("NormalMlStrategy must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy1(data, eInvestorProfile);

                case EStrategy.NormalMlStrategy2:
                    if (data.StockRightNow == null)
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

        public async Task<List<BetterCoinsTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            int limitOfDataToLearn = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();

            var results = new List<BetterCoinsTradeRightNowOutputDto>();

            foreach (var currency in allCurrencies)
            {
                if (currency == ECurrency.USDT)
                    continue;

                var data = GetRegressionData(currency, interval, limitOfDataToLearn, startTime, endTime);

                var regressionTestResult = await RegressionExec(strategy, eInvestorProfile, data);

                results.Add(new BetterCoinsTradeRightNowOutputDto
                {
                    Currency = currency,
                    WhatToDo = regressionTestResult.WhatToDo,
                    Data = data
                });
            }

            return results.Where(x => x.WhatToDo.WhatToDo == EWhatToDo.Buy).ToList();
        }

        public async Task<List<BetterCoinsTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            List<EStrategy> strategies,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            int limitOfDataToLearn = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            if (!strategies.Any())
            {
                throw new ArgumentException("Must to have at least one strategy");
            }

            var firstStrategy = strategies.First();
            strategies.Remove(firstStrategy);

            var result = await GetBetterCoinsToTraderRightNowAsync(firstStrategy, eInvestorProfile, interval, limitOfDataToLearn, startTime, endTime);

            foreach (var strategy in strategies)
            {
                result = await FilterBetterCoinsToTraderRightNowAsync(strategy, eInvestorProfile, result);
            }

            return result;
        }

        public async Task<List<BetterCoinsTradeRightNowOutputDto>> FilterBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            List<BetterCoinsTradeRightNowOutputDto> input)
        {
            var result = new List<BetterCoinsTradeRightNowOutputDto>();

            foreach (var item in input)
            {
                var regressionResult = await RegressionExec(strategy, eInvestorProfile, item.Data);

                if (regressionResult.WhatToDo.WhatToDo == EWhatToDo.Buy)
                {
                    result.Add(new BetterCoinsTradeRightNowOutputDto
                    {
                        Currency = item.Currency,
                        WhatToDo = regressionResult.WhatToDo,
                        Data = item.Data
                    });
                }
            }

            return result;
        }

        public RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            int limitOfDataToLearn = 120,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var sampleStock = _binanceService.GetKline($"{currency}{CryptoBotConsts.BaseCoinName}");
            var dataToLearn = _binanceService.GetData(currency, interval, startTime, endTime, limitOfDataToLearn);

            return new RegressionDataOutput
            {
                Currency = currency,
                StockRightNow = sampleStock,
                Interval = interval,
                LimitOfDataToLearn = limitOfDataToLearn,
                DataToLearn = dataToLearn
            };
        }

        public async Task<RegressionOutputDto> RegressionExec(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionDataOutput data)
        {
            var resultTraderService = await WhatToDo(strategy, eInvestorProfile, data);

            return new RegressionOutputDto
            {
                ActualStock = data.StockRightNow,
                WhatToDo = resultTraderService,
            };
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
            var result = await _normalMlStrategy1.ShouldBuyStock(data.DataToLearn, eInvestorProfile, data.StockRightNow);

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
            var result = await _normalMlStrategy2.ShouldBuyStock(data.DataToLearn, eInvestorProfile, data.StockRightNow);

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
    }
}
