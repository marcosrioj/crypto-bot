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
        public readonly IRepository<Order, long> _orderRepository;
        public readonly IRepository<PredictionOrder, long> _predictionOrderRepository;
        public readonly IRepository<Prediction, long> _predictionRepository;

        private readonly IBinanceService _binanceService;
        private readonly IWalletService _walletService;
        private readonly IMLStrategy1 _normalMlStrategy1;
        private readonly IMLStrategy2 _normalMlStrategy2;
        private readonly IMeanReversionStrategy _simpleMeanReversionStrategy;
        private readonly IMicrotrendStrategy _simpleMicrotrendStrategy;
        private readonly Strategies.Simple.MLStrategy1.IMLStrategy1 _simpleMlStrategy1;

        public TraderService(
            IRepository<Trading, long> tradingRepository,
            IRepository<Order, long> orderRepository,
            IRepository<Prediction, long> predictionRepository,
            IRepository<PredictionOrder, long> predictionOrderRepository,
            IBinanceService binanceService,
            IWalletService walletService,
            IMLStrategy1 normalMlStrategy1,
            IMLStrategy2 normalMlStrategy2,
            IMeanReversionStrategy simpleMeanReversionStrategy,
            IMicrotrendStrategy simpleMicrotrendStrategy,
            Strategies.Simple.MLStrategy1.IMLStrategy1 simpleMlStrategy1)
        {
            _tradingRepository = tradingRepository;
            _orderRepository = orderRepository;
            _predictionRepository = predictionRepository;
            _predictionOrderRepository = predictionOrderRepository;

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

        public async Task GenerateBetterPrediction1Async(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            int limitOfDataToLearn = 1000)
        {
            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();

            foreach (var currency in allCurrencies)
            {
                if (currency == ECurrency.USDT)
                    continue;

                var data = GetRegressionData(currency, interval, limitOfDataToLearn);

                var whatToDo = await WhatToDo(strategy, eInvestorProfile, data);

                if (whatToDo.WhatToDo == EWhatToDo.Buy)
                {
                    await _predictionRepository.InsertAndGetIdAsync(new Prediction
                    {
                        CreationTime = DateTime.Now,
                        Currency = currency,
                        InvestorProfile = eInvestorProfile,
                        Type = EPredictionType.Regression1,
                        Strategy = strategy,
                        WhatToDo = whatToDo.WhatToDo,
                        Interval = interval,
                        Score = whatToDo.Score,
                        DataLearned = limitOfDataToLearn
                    });
                }
            }
        }

        public async Task GenerateBetterPrediction2Async(
            List<EStrategy> strategies,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            int limitOfDataToLearn = 1000)
        {
            if (!strategies.Any())
            {
                throw new ArgumentException("Must to have at least one strategy");
            }

            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();
            var firstStrategy = strategies.First();
            strategies.Remove(firstStrategy);

            foreach (var currency in allCurrencies)
            {
                if (currency == ECurrency.USDT)
                    continue;

                var data = GetRegressionData(currency, interval, limitOfDataToLearn);

                var whatToDo = await WhatToDo(firstStrategy, eInvestorProfile, data);

                if (whatToDo.WhatToDo == EWhatToDo.Buy)
                {
                    var runnedAllStrategies = true;
                    var strategiesSb = new StringBuilder($"{firstStrategy}.");

                    for (int i = 0; i < strategies.Count; i++)
                    {
                        var strategy = strategies[i];
                        strategiesSb.Append($"{strategy}.");
                        var whatToDoByStrategy = await WhatToDo(strategy, eInvestorProfile, data);

                        if (whatToDoByStrategy.WhatToDo != EWhatToDo.Buy)
                        {
                            runnedAllStrategies = false;
                            i = strategies.Count;
                        }
                    }

                    if (runnedAllStrategies)
                    {
                        await _predictionRepository.InsertAndGetIdAsync(new Prediction
                        {
                            CreationTime = DateTime.Now,
                            Currency = currency,
                            InvestorProfile = eInvestorProfile,
                            Type = EPredictionType.Regression2,
                            Strategies = strategiesSb.ToString(),
                            WhatToDo = whatToDo.WhatToDo,
                            Interval = interval,
                            Score = whatToDo.Score,
                            DataLearned = limitOfDataToLearn
                        });
                    }
                }
            }
        }

        public RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            int limitOfDataToLearn = 120)
        {
            var sampleStock = _binanceService.GetKline($"{currency}{CryptoBotConsts.BaseCoinName}");
            var dataToLearn = _binanceService.GetData(currency, interval, null, null, limitOfDataToLearn);

            return new RegressionDataOutput
            {
                Currency = currency,
                StockRightNow = sampleStock,
                Interval = interval,
                LimitOfDataToLearn = limitOfDataToLearn,
                DataToLearn = dataToLearn
            };
        }

        public async Task AutoTraderWithWalletVirtualAsync(long userId)
        {
            var mainWallet = await _walletService.GetOrCreate(ECurrency.USDT, EWalletType.Virtual, userId, 1000);

            if (mainWallet.Balance < 300)
            {
                return;
            }

            //var maindTragindId = await CreateTrading(mainWallet.Id, mainWallet.Balance);

            var predictions = await _predictionRepository
                .GetAll()
                .AsNoTracking()
                .Where(x =>
                    x.CreationTime > DateTime.Now.AddSeconds(-10)
                    && !x.Orders.Any(y => y.CreatorUserId == userId))
                .ToListAsync();

            if (predictions.Count == 0)
            {
                return;
            }

            var predictionsFiltered = new List<PredictionFilteredDto>();
            foreach (var prediction in predictions)
            {
                var pair = $"{prediction.Currency}{CryptoBotConsts.BaseCoinName}";
                var bookPrice = _binanceService.GetBookPrice(pair);
                var bookOrder = _binanceService.GetBookOrders(pair, 20);

                if (bookPrice.Data != null
                    && bookOrder.Data != null
                    && bookOrder.Data.Asks.Any()
                    && bookOrder.Data.Asks.Any())
                {
                    var askQuantity = bookOrder.Data.Asks.Sum(x => x.Quantity);
                    var bidQuantity = bookOrder.Data.Bids.Sum(x => x.Quantity);

                    if (bidQuantity > askQuantity)
                    {
                        if (mainWallet.Balance < bidQuantity * bookPrice.Data.BestAskPrice)
                        {
                            predictionsFiltered.Add(new PredictionFilteredDto
                            {
                                Prediction = prediction,
                                BookPrice = bookPrice.Data,
                                AsksData = bookOrder.Data.Asks,
                                BidsData = bookOrder.Data.Bids
                            });
                        }
                    }
                }
            }

            if (predictionsFiltered.Count == 0)
            {
                return;
            }

            var percApprovedByOperation = 0.2m; //20%
            var balanceUsdtCalc = (mainWallet.Balance * percApprovedByOperation) / predictionsFiltered.Count;

            foreach (var prediction in predictionsFiltered)
            {
                var amount = balanceUsdtCalc / prediction.BookPrice.BestAskPrice;

                await _predictionOrderRepository.InsertAndGetIdAsync(
                    new PredictionOrder
                    {
                        Order = new Order
                        {
                            From = ECurrency.USDT,
                            To = prediction.Prediction.Currency,
                            UsdtPriceFrom = 1,
                            UsdtPriceTo = prediction.BookPrice.BestAskPrice,
                            CreatorUserId = userId,
                            Amount = amount
                        },
                        PredictionId = prediction.Prediction.Id,
                        CreatorUserId = userId
                    });

                var wallet = await _walletService.GetOrCreate(prediction.Prediction.Currency, EWalletType.Virtual, userId);
                var newWalletBalance = wallet.Balance + amount;
                await _walletService.UpdateBalance(wallet.Id, newWalletBalance);

                mainWallet = await _walletService.GetOrCreate(ECurrency.USDT, EWalletType.Virtual, userId);
                var newMainWalletBalance = mainWallet.Balance - balanceUsdtCalc;
                await _walletService.UpdateBalance(mainWallet.Id, newMainWalletBalance);

                //var sb = new StringBuilder();
                //sb.AppendLine(coinToTrade.Currency.ToString());
                //sb.AppendLine($"BestAskPrice: {bookPrice.Data.BestAskPrice}");
                //sb.AppendLine($"BestAskQuantity: {bookPrice.Data.BestAskQuantity}");
                //sb.AppendLine($"BestBidPrice: {bookPrice.Data.BestBidPrice}");
                //sb.AppendLine($"BestBidQuantity: {bookPrice.Data.BestBidQuantity}");
                //sb.AppendLine($"askQuantity: {askQuantity}");
                //sb.AppendLine($"bidQuantity: {bidQuantity}\n\n");

                //LogHelper.Log($"{sb}", "test");
            }
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
