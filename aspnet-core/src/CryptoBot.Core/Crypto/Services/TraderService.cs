using Abp.Configuration;
using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Strategies;
using CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy;
using CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy;
using CryptoBot.Crypto.Strategies.Simple.MLStrategy;

namespace CryptoBot.Crypto.Services
{
    public class TraderService : DomainService, ITraderService
    {
        private readonly ISettingManager _settingManager;
        private readonly IBinanceService _binanceService;

        private IEnumerable<IBinanceKline> _inputData;

        public TraderService(
            ISettingManager settingManager,
            IBinanceService binanceService)
        {
            _settingManager = settingManager;
            _binanceService = binanceService;
        }

        public Task<EWhatToDo> WhatToDo(
            ECurrency currency,
            KlineInterval interval,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int limitOfDetails = 1000)
        {
            return Task.FromResult(EWhatToDo.Buy);
        }

        public async Task<EWhatToDo> WhatToDo(
            EStrategy strategy,
            ECurrency currency,
            KlineInterval interval,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int limitOfDetails = 1000)
        {
            SetData(currency, interval, startTime, endTime, limitOfDetails);

            switch (strategy)
            {
                case EStrategy.SimpleMlStrategy:
                    return await WhatToDoBySimpleMlStrategy(currency);

                case EStrategy.SimpleMeanReversionStrategy:
                    return await WhatToDoBySimpleMeanReversionStrategy(currency);

                case EStrategy.SimpleMicrotrendStrategy:
                    return await WhatToDoBySimpleMicrotrendStrategy(currency);

                default:
                    throw new Exception("Strategy not found");
            }
        }

        private async Task<EWhatToDo> WhatToDoBySimpleMeanReversionStrategy(ECurrency currency)
        {
            var strategy = new MeanReversionStrategy();
            var result = await ShouldBuyStockBySimple(strategy, currency);

            if (result.HasValue)
            {
                if (result.Value)
                {
                    return await Task.FromResult(EWhatToDo.Buy);
                }

                //TODO: Create logic to sell
                return await Task.FromResult(EWhatToDo.Hold);
            }

            return await Task.FromResult(EWhatToDo.Hold);
        }

        private async Task<EWhatToDo> WhatToDoBySimpleMicrotrendStrategy(ECurrency currency)
        {
            var strategy = new MicrotrendStrategy();
            var result = await ShouldBuyStockBySimple(strategy, currency);

            if (result.HasValue)
            {
                if (result.Value)
                {
                    return await Task.FromResult(EWhatToDo.Buy);
                }

                //TODO: Create logic to sell
                return await Task.FromResult(EWhatToDo.Hold);
            }

            return await Task.FromResult(EWhatToDo.Hold);
        }

        private async Task<EWhatToDo> WhatToDoBySimpleMlStrategy(ECurrency currency)
        {
            var strategy = new MLStrategy();
            var result = await ShouldBuyStockBySimple(strategy, currency);

            if (result.HasValue)
            {
                if (result.Value)
                {
                    return await Task.FromResult(EWhatToDo.Buy);
                }

                //TODO: Create logic to sell
                return await Task.FromResult(EWhatToDo.Hold);
            }

            return await Task.FromResult(EWhatToDo.Hold);
        }

        private void SetData(ECurrency currency, KlineInterval interval, DateTime? startTime, DateTime? endTime,
            int limitOfDetails)
        {
            var pair = $"{currency}{CryptoBotConsts.BaseCoinName}";

            if (_inputData == null)
            {
                var klinesResult = _binanceService.GetKlines(pair, interval, limitOfDetails, startTime, endTime);

                if (klinesResult.Success)
                {
                    _inputData = klinesResult.Data;
                }
            }
        }

        private async Task<bool?> ShouldBuyStockBySimple(IStrategy strategy, ECurrency currency)
        {
            var input = _inputData.Select(x => new StockInput
            {
                ClosingPrice = x.Close,
                StockSymbol = currency.ToString(),
                Time = x.CloseTime
            }).ToList();

            var result = await strategy.ShouldBuyStock(input);
            return result;
        }

        public void LogFull(string message)
        {
            using (StreamWriter writer = new StreamWriter("trader_full.log", true, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(message);
            }
        }

        public void LogMain(string message)
        {
            using (StreamWriter writer = new StreamWriter("trader_main.log", true, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(message);
            }
        }
    }
}
