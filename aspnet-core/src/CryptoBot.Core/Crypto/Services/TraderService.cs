using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy1;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy2;
using CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy;
using CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBot.Crypto.Dtos.Services;
using Abp.Quartz;
using Quartz;
using CryptoBot.Crypto.Background.Jobs;
using Abp.ObjectMapping;
using Abp.Domain.Uow;

namespace CryptoBot.Crypto.Services
{
    public class TraderService : DomainService, ITraderService
    {
        public readonly IRepository<Order, long> _orderRepository;
        public readonly IRepository<PredictionOrder, long> _predictionOrderRepository;
        public readonly IRepository<Prediction, long> _predictionRepository;
        public readonly IRepository<Formula, long> _formulaRepository;

        private readonly IBinanceService _binanceService;
        private readonly IWalletService _walletService;
        private readonly IMLStrategy1 _normalMlStrategy1;
        private readonly IMLStrategy2 _normalMlStrategy2;
        private readonly IMeanReversionStrategy _simpleMeanReversionStrategy;
        private readonly IMicrotrendStrategy _simpleMicrotrendStrategy;
        private readonly Strategies.Simple.MLStrategy1.IMLStrategy1 _simpleMlStrategy1;

        private readonly IQuartzScheduleJobManager _jobManager;
        private readonly IObjectMapper _objectMapper;

        public TraderService(
            IRepository<Order, long> orderRepository,
            IRepository<Prediction, long> predictionRepository,
            IRepository<PredictionOrder, long> predictionOrderRepository,
            IRepository<Formula, long> formulaRepository,
            IBinanceService binanceService,
            IWalletService walletService,
            IMLStrategy1 normalMlStrategy1,
            IMLStrategy2 normalMlStrategy2,
            IMeanReversionStrategy simpleMeanReversionStrategy,
            IMicrotrendStrategy simpleMicrotrendStrategy,
            Strategies.Simple.MLStrategy1.IMLStrategy1 simpleMlStrategy1,
            IQuartzScheduleJobManager jobManager,
            IObjectMapper objectMapper)
        {
            _orderRepository = orderRepository;
            _predictionRepository = predictionRepository;
            _predictionOrderRepository = predictionOrderRepository;
            _formulaRepository = formulaRepository;

            _binanceService = binanceService;
            _walletService = walletService;
            _normalMlStrategy1 = normalMlStrategy1;
            _normalMlStrategy2 = normalMlStrategy2;
            _simpleMeanReversionStrategy = simpleMeanReversionStrategy;
            _simpleMicrotrendStrategy = simpleMicrotrendStrategy;
            _simpleMlStrategy1 = simpleMlStrategy1;

            _jobManager = jobManager;
            _objectMapper = objectMapper;
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

        [UnitOfWork(false)]
        public async Task GenerateBetterPredictionsAsync(FormulaDto formula)
        {
            if (!Enum.IsDefined(formula.Strategy1))
            {
                throw new ArgumentException("Must to have at least one strategy");
            }

            var allCurrencies = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();

            foreach (var currency in allCurrencies)
            {
                if (currency == ECurrency.USDT)
                    continue;

                var data = GetRegressionData(currency, formula.IntervalToBuy, formula.LimitOfDataToLearn);

                var whatToDo = await WhatToDo(formula.Strategy1, formula.InvestorProfile1, data);

                if (whatToDo.WhatToDo == EWhatToDo.Buy)
                {
                    if (formula.Strategy2.HasValue && formula.InvestorProfile2.HasValue)
                    {
                        whatToDo = await WhatToDo(formula.Strategy2.Value, formula.InvestorProfile2.Value, data);

                        if (whatToDo.WhatToDo == EWhatToDo.Buy
                            && formula.Strategy3.HasValue
                            && formula.InvestorProfile3.HasValue)
                        {
                            whatToDo = await WhatToDo(formula.Strategy3.Value, formula.InvestorProfile3.Value, data);
                        }
                    }

                    if (whatToDo.WhatToDo == EWhatToDo.Buy)
                    {
                        await _predictionRepository.InsertAndGetIdAsync(new Prediction
                        {
                            CreationTime = DateTime.Now,
                            Currency = currency,
                            Strategy1 = formula.Strategy1,
                            InvestorProfile1 = formula.InvestorProfile1,
                            Strategy2 = formula.Strategy2,
                            InvestorProfile2 = formula.InvestorProfile2,
                            Strategy3 = formula.Strategy3,
                            InvestorProfile3 = formula.InvestorProfile3,
                            WhatToDo = whatToDo.WhatToDo,
                            IntervalToBuy = formula.IntervalToBuy,
                            IntervalToSell = formula.IntervalToSell,
                            Score = whatToDo.Score.ToString(),
                            DataLearned = formula.LimitOfDataToLearn
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

        [UnitOfWork(false)]
        public async Task AutoTraderBuyWithWalletVirtualAsync(long userId, FormulaDto formula)
        {
            var mainWallet = await _walletService.GetOrCreate(ECurrency.USDT, EWalletType.Virtual, userId, 1000);

            if (mainWallet.Balance < formula.BalancePreserved)
            {
                return;
            }

            var predictions = await _predictionRepository
                .GetAll()
                .AsNoTracking()
                .Where(x =>
                    x.CreationTime > DateTime.Now.AddSeconds(-10)
                    && !x.Orders.Any(y => y.CreatorUserId == userId)
                    && x.IntervalToBuy == formula.IntervalToBuy
                    && x.Strategy1 == formula.Strategy1
                    && x.InvestorProfile1 == formula.InvestorProfile1)
                .WhereIf(
                    formula.Strategy2.HasValue && formula.InvestorProfile2.HasValue,
                    x => x.Strategy2 == formula.Strategy2
                            && x.InvestorProfile2 == formula.InvestorProfile2)
                .WhereIf(
                    formula.Strategy3.HasValue && formula.InvestorProfile3.HasValue,
                    x => x.Strategy3 == formula.Strategy3
                            && x.InvestorProfile3 == formula.InvestorProfile3)
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

                    var bookOrdersVerified = false;

                    switch (formula.BookOrdersAction)
                    {
                        case EBookOrdersAction.BidGreaterThanAsk:
                            bookOrdersVerified = bidQuantity > (askQuantity + (askQuantity * formula.BookOrdersFactor));
                            break;
                        case EBookOrdersAction.AskGreaterThanBid:
                            bookOrdersVerified = askQuantity > (bidQuantity + (bidQuantity * formula.BookOrdersFactor));
                            break;
                        case EBookOrdersAction.None:
                            bookOrdersVerified = true;
                            break;
                        default:
                            bookOrdersVerified = false;
                            break;
                    }

                    if (bookOrdersVerified)
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

            var priceUsdtToBuyByPredictionFiltered = 0m;

            if (formula.OrderPriceType == EOrderPriceType.Percent)
            {
                priceUsdtToBuyByPredictionFiltered = (mainWallet.Balance * formula.OrderPrice) / predictionsFiltered.Count;
            } 
            else
            {
                priceUsdtToBuyByPredictionFiltered = formula.OrderPrice / predictionsFiltered.Count;
            }

            foreach (var prediction in predictionsFiltered)
            {
                var amount = priceUsdtToBuyByPredictionFiltered / prediction.BookPrice.BestAskPrice;

                var predictionOrderId = await _predictionOrderRepository.InsertAndGetIdAsync(
                    new PredictionOrder
                    {
                        Order = new Order
                        {
                            From = ECurrency.USDT,
                            To = prediction.Prediction.Currency,
                            Status = EOrderStatus.Buyed,
                            UsdtPriceFrom = 1,
                            UsdtPriceTo = prediction.BookPrice.BestAskPrice,
                            CreatorUserId = userId,
                            Amount = amount
                        },
                        PredictionId = prediction.Prediction.Id,
                        CreatorUserId = userId
                    });

                await _walletService.UpdatedWalletsUsdtToCustomCurrency(userId, priceUsdtToBuyByPredictionFiltered, prediction.Prediction.Currency, amount);
            }
        }

        [UnitOfWork(false)]
        public async Task AutoTraderSellWithWalletVirtualAsync()
        {
            var secondsEarlier = 20;

            var predictionOrders = await _predictionOrderRepository
                .GetAll()
                .Include(x => x.Order)
                .Include(x => x.Prediction)
                .Where(x =>
                    x.Order.Status == EOrderStatus.Buyed
                    && (
                        x.Prediction.IntervalToSell == KlineInterval.OneMinute
                        || (x.Prediction.IntervalToSell == KlineInterval.ThreeMinutes && x.CreationTime < DateTime.Now.AddSeconds(-180 + secondsEarlier))
                        || (x.Prediction.IntervalToSell == KlineInterval.FiveMinutes && x.CreationTime < DateTime.Now.AddSeconds(-300 + secondsEarlier))
                        || (x.Prediction.IntervalToSell == KlineInterval.FifteenMinutes && x.CreationTime < DateTime.Now.AddSeconds(-900 + secondsEarlier))
                        || (x.Prediction.IntervalToSell == KlineInterval.ThirtyMinutes && x.CreationTime < DateTime.Now.AddSeconds(-1800 + secondsEarlier))
                        || (x.Prediction.IntervalToSell == KlineInterval.OneHour && x.CreationTime < DateTime.Now.AddSeconds(-3600 + secondsEarlier))
                        || (x.Prediction.IntervalToSell == KlineInterval.TwoHour && x.CreationTime < DateTime.Now.AddSeconds(-7200 + secondsEarlier))
                    ))
                .ToListAsync();

            foreach (var predictionOrder in predictionOrders)
            {
                var pair = $"{predictionOrder.Order.To}{CryptoBotConsts.BaseCoinName}";
                var bookPrice = _binanceService.GetBookPrice(pair);

                var amount = bookPrice.Data.BestBidPrice * predictionOrder.Order.Amount;

                var predictionOrderId = await _orderRepository.InsertAndGetIdAsync(
                    new Order
                    {
                        From = predictionOrder.Order.To,
                        To = ECurrency.USDT,
                        Status = EOrderStatus.Selled,
                        UsdtPriceFrom = bookPrice.Data.BestBidPrice,
                        UsdtPriceTo = 1,
                        CreatorUserId = predictionOrder.CreatorUserId,
                        Amount = amount,
                        OriginOrderId = predictionOrder.OrderId
                    });

                await _walletService.UpdatedWalletsCustomCurrencyToUsdt(predictionOrder.CreatorUserId.Value, predictionOrder.Order.Amount, predictionOrder.Order.To, amount);

                predictionOrder.Order.Status = EOrderStatus.Selled;
            }

        }

        public async Task ScheduleAutoTraderSellWithWalletVirtualAsync()
        {
            await _jobManager.ScheduleAsync<SellVirtualTraderJob>(
                job =>
                {
                    job.WithIdentity("SellTrader");
                },
                trigger =>
                {
                    trigger
                        .StartNow()
                        .WithCronSchedule("50 * * * * ?");
                });
        }

        [UnitOfWork(false)]
        public async Task StartScheduleFormulas()
        {
            var formulas = await _formulaRepository
                .GetAll()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync();

            foreach (var formula in formulas)
            {
                var dto = _objectMapper.Map<FormulaDto>(formula);

                await ScheduleGeneratePredictions(_objectMapper.Map<FormulaDto>(dto));
                await ScheduleBuyVirtualTrader(3, dto);
            }
        }

        public async Task UnscheduleGeneratePredictions(long formulaId)
        {
            await _jobManager.UnscheduleAsync(new TriggerKey($"GeneratePredictionsJob-{formulaId}"));
        }

        public async Task UnscheduleBuyVirtualTrader(long userId, long formulaId)
        {
            await _jobManager.UnscheduleAsync(new TriggerKey($"BuyVirtualTraderJob-User-{userId}-Formula-{formulaId}"));
        }

        public async Task ScheduleGeneratePredictions(FormulaDto formula)
        {
            await _jobManager.ScheduleAsync<GeneratePredictionsJob>(
                job =>
                {
                    job
                        .WithIdentity($"GeneratePredictionsJob-{formula.Id}")
                        .UsingJobData("Id", formula.Id)
                        .UsingJobData("IntervalToBuy", (int)formula.IntervalToBuy)
                        .UsingJobData("IntervalToSell", (int)formula.IntervalToSell)
                        .UsingJobData("LimitOfDataToLearn", formula.LimitOfDataToLearn)
                        .UsingJobData("Strategy1", (int)formula.Strategy1)
                        .UsingJobData("InvestorProfile1", (int)formula.InvestorProfile1)
                        .UsingJobData("Strategy2", formula.Strategy2.HasValue ? (int)formula.Strategy2 : 0)
                        .UsingJobData("InvestorProfile2", formula.InvestorProfile2.HasValue ? (int)formula.InvestorProfile2 : 0)
                        .UsingJobData("Strategy3", formula.Strategy3.HasValue ? (int)formula.Strategy3 : 0)
                        .UsingJobData("InvestorProfile3", formula.InvestorProfile3.HasValue ? (int)formula.InvestorProfile3 : 0)
                        .UsingJobData("BalancePreserved", (float)formula.BalancePreserved)
                        .UsingJobData("OrderPrice", (float)formula.OrderPrice)
                        .UsingJobData("OrderPriceType", (int)formula.OrderPriceType)
                        .UsingJobData("Description", formula.Description)
                        .UsingJobData("BookOrdersAction", (int)formula.BookOrdersAction)
                        .UsingJobData("BookOrdersFactor", (float)formula.BookOrdersFactor); ;
                },
                trigger =>
                {
                    trigger
                        .StartNow()
                        .WithSimpleSchedule(schedule =>
                        {
                            schedule.RepeatForever()
                                .WithIntervalInSeconds(1)
                                .Build();
                        });
                });
        }

        public async Task ScheduleBuyVirtualTrader(long userId, FormulaDto formula)
        {
            var oneMinuteAfter = DateTimeOffset.Now.AddMinutes(1);

            await _jobManager.ScheduleAsync<BuyVirtualTraderJob>(
                job =>
                {
                    job
                        .WithIdentity($"BuyVirtualTraderJob-User-{userId}-Formula-{formula.Id}")
                        .UsingJobData("UserId", userId)
                        .UsingJobData("Id", formula.Id)
                        .UsingJobData("IntervalToBuy", (int)formula.IntervalToBuy)
                        .UsingJobData("IntervalToSell", (int)formula.IntervalToSell)
                        .UsingJobData("LimitOfDataToLearn", formula.LimitOfDataToLearn)
                        .UsingJobData("Strategy1", (int)formula.Strategy1)
                        .UsingJobData("InvestorProfile1", (int)formula.InvestorProfile1)
                        .UsingJobData("Strategy2", formula.Strategy2.HasValue ? (int)formula.Strategy2 : 0)
                        .UsingJobData("InvestorProfile2", formula.InvestorProfile2.HasValue ? (int)formula.InvestorProfile2 : 0)
                        .UsingJobData("Strategy3", formula.Strategy3.HasValue ? (int)formula.Strategy3 : 0)
                        .UsingJobData("InvestorProfile3", formula.InvestorProfile3.HasValue ? (int)formula.InvestorProfile3 : 0)
                        .UsingJobData("BalancePreserved", (float)formula.BalancePreserved)
                        .UsingJobData("OrderPrice", (float)formula.OrderPrice)
                        .UsingJobData("OrderPriceType", (int)formula.OrderPriceType)
                        .UsingJobData("Description", formula.Description)
                        .UsingJobData("BookOrdersAction", (int)formula.BookOrdersAction)
                        .UsingJobData("BookOrdersFactor", (float)formula.BookOrdersFactor);
                },
                trigger =>
                {
                    trigger
                        .StartAt(new DateTimeOffset(
                            oneMinuteAfter.Year,
                            oneMinuteAfter.Month,
                            oneMinuteAfter.Day,
                            oneMinuteAfter.Hour,
                            oneMinuteAfter.Minute,
                            0,
                            oneMinuteAfter.Offset))
                        .WithSimpleSchedule(schedule =>
                        {
                            schedule.RepeatForever()
                                .WithIntervalInSeconds(1)
                                .Build();
                        });
                });
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
