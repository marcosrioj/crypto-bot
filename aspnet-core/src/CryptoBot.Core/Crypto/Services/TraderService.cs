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
using CryptoBot.Crypto.Services.Dtos;

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
            RegressionTestDataOutput data)
        {
            return await Task.FromResult(new WhatToDoOutput());
        }

        public async Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
            RegressionTestDataOutput data)
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


        private async Task<WhatToDoOutput> WhatToDoBySimpleMeanReversionStrategy(RegressionTestDataOutput data)
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

        private async Task<WhatToDoOutput> WhatToDoBySimpleMicrotrendStrategy(RegressionTestDataOutput data)
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

        private async Task<WhatToDoOutput> WhatToDoBySimpleMlStrategy1(RegressionTestDataOutput data)
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

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy1(RegressionTestDataOutput data)
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

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy2(RegressionTestDataOutput data)
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
    }
}
