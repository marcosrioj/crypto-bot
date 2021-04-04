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
using CryptoBot.Crypto.Strategies.Normal.MLStrategy1;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy2;
using CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy;
using CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy;
using CryptoBot.Crypto.Strategies.Simple.MLStrategy1;

namespace CryptoBot.Crypto.Services
{
    public class TraderService : DomainService, ITraderService
    {
        private readonly IBinanceService _binanceService;

        private Dictionary<ECurrency, List<IBinanceKline>> _inputData;

        public TraderService(
            IBinanceService binanceService)
        {
            _binanceService = binanceService;
            _inputData = new Dictionary<ECurrency, List<IBinanceKline>>();
        }

        public Task<EWhatToDo> WhatToDo(
            ECurrency currency)
        {
            return Task.FromResult(EWhatToDo.Buy);
        }

        public async Task<EWhatToDo> WhatToDo(
            EStrategy strategy,
            ECurrency currency,
            IBinanceKline? sampleStock = null)
        {
            switch (strategy)
            {
                case EStrategy.NormalMlStrategy1:
                    if (sampleStock == null)
                    {
                        throw new Exception("NormalMlStrategy must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy1(currency, sampleStock);

                case EStrategy.NormalMlStrategy2:
                    if (sampleStock == null)
                    {
                        throw new Exception("NormalMlStrategy2 must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy2(currency, sampleStock);

                case EStrategy.SimpleMlStrategy1:
                    return await WhatToDoBySimpleMlStrategy1(currency);

                case EStrategy.SimpleMeanReversionStrategy:
                    return await WhatToDoBySimpleMeanReversionStrategy(currency);

                case EStrategy.SimpleMicrotrendStrategy:
                    return await WhatToDoBySimpleMicrotrendStrategy(currency);

                default:
                    throw new Exception("Strategy not found");
            }
        }

        public void SetData(ECurrency currency, KlineInterval interval, DateTime? startTime, DateTime? endTime, int limitOfDetails)
        {
            var inputData = _binanceService.GetData(currency, interval, startTime, endTime, limitOfDetails);

            SetData(currency, inputData);
        }

        public void SetData(ECurrency currency, List<IBinanceKline> inputData)
        {
            if (inputData != null)
            {
                _inputData[currency] = inputData;
            }
        }

        private async Task<EWhatToDo> WhatToDoBySimpleMeanReversionStrategy(ECurrency currency)
        {
            var strategy = new MeanReversionStrategy();
            var result = await strategy.ShouldBuyStock(_inputData[currency]);

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
            var result = await strategy.ShouldBuyStock(_inputData[currency]);

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

        private async Task<EWhatToDo> WhatToDoBySimpleMlStrategy1(ECurrency currency)
        {
            var strategy = new Strategies.Simple.MLStrategy1.MLStrategy1();
            var result = await strategy.ShouldBuyStock(_inputData[currency]);

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

        private async Task<EWhatToDo> WhatToDoByNormalMlStrategy1(ECurrency currency, IBinanceKline sampleStock)
        {
            var strategy = new Strategies.Normal.MLStrategy1.MLStrategy1();

            var result = await strategy.ShouldBuyStock(_inputData[currency], sampleStock);

            if (result)
            {
                return await Task.FromResult(EWhatToDo.Buy);
            }

            //TODO: Create logic to sell
            return await Task.FromResult(EWhatToDo.Hold);
        }

        private async Task<EWhatToDo> WhatToDoByNormalMlStrategy2(ECurrency currency, IBinanceKline sampleStock)
        {
            var strategy = new MLStrategy2();

            var result = await strategy.ShouldBuyStock(_inputData[currency], sampleStock);

            if (result)
            {
                return await Task.FromResult(EWhatToDo.Buy);
            }

            //TODO: Create logic to sell
            return await Task.FromResult(EWhatToDo.Hold);
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
