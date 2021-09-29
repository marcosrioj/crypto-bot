using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.ObjectMapping;
using Abp.Quartz;
using Binance.Net.Enums;
using CryptoBot.Crypto.Background.Jobs;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy1;
using CryptoBot.Crypto.Strategies.Normal.MLStrategy2;
using CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy;
using CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class TraderService : DomainService, ITraderService
    {
        public readonly IRepository<Order, long> _orderRepository;
        public readonly IRepository<PredictionOrder, long> _predictionOrderRepository;
        public readonly IRepository<Prediction, long> _predictionRepository;
        public readonly IRepository<Formula, long> _formulaRepository;
        public readonly IRepository<Robot, long> _robotRepository;

        private readonly IBinanceService _binanceService;
        private readonly IWalletService _walletService;
        private readonly IMLStrategy1 _normalMlStrategy1;
        private readonly IMLStrategy2 _normalMlStrategy2;
        private readonly IMeanReversionStrategy _simpleMeanReversionStrategy;
        private readonly IMicrotrendStrategy _simpleMicrotrendStrategy;
        private readonly ISimpleRsiStrategy _simpleStrategyRsi;
        private readonly Strategies.Simple.MLStrategy1.IMLStrategy1 _simpleMlStrategy1;

        private readonly IQuartzScheduleJobManager _jobManager;
        private readonly IObjectMapper _objectMapper;

        public TraderService(
            IRepository<Order, long> orderRepository,
            IRepository<Prediction, long> predictionRepository,
            IRepository<PredictionOrder, long> predictionOrderRepository,
            IRepository<Formula, long> formulaRepository,
            IRepository<Robot, long> robotRepository,
            IBinanceService binanceService,
            IWalletService walletService,
            IMLStrategy1 normalMlStrategy1,
            IMLStrategy2 normalMlStrategy2,
            IMeanReversionStrategy simpleMeanReversionStrategy,
            IMicrotrendStrategy simpleMicrotrendStrategy,
            ISimpleRsiStrategy simpleStrategyRsi,
            Strategies.Simple.MLStrategy1.IMLStrategy1 simpleMlStrategy1,
            IQuartzScheduleJobManager jobManager,
            IObjectMapper objectMapper)
        {
            _orderRepository = orderRepository;
            _predictionRepository = predictionRepository;
            _predictionOrderRepository = predictionOrderRepository;
            _formulaRepository = formulaRepository;
            _robotRepository = robotRepository;

            _binanceService = binanceService;
            _walletService = walletService;
            _normalMlStrategy1 = normalMlStrategy1;
            _normalMlStrategy2 = normalMlStrategy2;
            _simpleMeanReversionStrategy = simpleMeanReversionStrategy;
            _simpleMicrotrendStrategy = simpleMicrotrendStrategy;
            _simpleMlStrategy1 = simpleMlStrategy1;
            _simpleStrategyRsi = simpleStrategyRsi;

            _jobManager = jobManager;
            _objectMapper = objectMapper;
        }

        public async Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            EProfitWay profitWay,
            RegressionDataOutput data)
        {
            switch (strategy)
            {
                case EStrategy.NormalMlStrategy1:
                    if (data.StockRightNow == null)
                    {
                        throw new Exception("NormalMlStrategy must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy1(data, profitWay, eInvestorProfile);

                case EStrategy.NormalMlStrategy2:
                    if (data.StockRightNow == null)
                    {
                        throw new Exception("NormalMlStrategy2 must to have the actual stock");
                    }
                    return await WhatToDoByNormalMlStrategy2(data, profitWay, eInvestorProfile);

                case EStrategy.SimpleMlStrategy1:
                    return await WhatToDoBySimpleMlStrategy1(data, profitWay, eInvestorProfile);

                case EStrategy.SimpleMeanReversionStrategy:
                    return await WhatToDoBySimpleMeanReversionStrategy(data, profitWay, eInvestorProfile);

                case EStrategy.SimpleMicrotrendStrategy:
                    return await WhatToDoBySimpleMicrotrendStrategy(data, profitWay, eInvestorProfile);

                case EStrategy.SimpleRsiStrategy:
                    return await WhatToDoBySimpleRsiStrategy(data, profitWay, eInvestorProfile);

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

            var allCurrencies = CurrencyHelper.GetCurrencies(formula.Currencies);

            foreach (var currency in allCurrencies)
            {
                if (currency == ECurrency.USDT)
                    continue;

                await ScheduleGeneratePrediction(formula, currency);
            }
        }

        public async Task<List<GetDecisionsOutputDto>> GetDecisionsAsync(FormulaDto formula)
        {
            if (!Enum.IsDefined(formula.Strategy1))
            {
                throw new ArgumentException("Must to have at least one strategy");
            }

            var allCurrencies = CurrencyHelper.GetCurrencies(formula.Currencies);

            var result = new List<GetDecisionsOutputDto>();

            foreach (var currency in allCurrencies)
            {
                if (currency == ECurrency.USDT)
                    continue;

                var whatTodo = await GetDecisionAsync(formula, currency);

                if (whatTodo == null)
                {
                    continue;
                }

                if (whatTodo.WhatToDo == EWhatToDo.Buy)
                {
                    result.Add(new GetDecisionsOutputDto
                    {
                        Currency = currency.ToString(),
                        Buy = true
                    });
                }
            }

            return result;
        }

        public async Task GenerateBetterPredictionAsync(FormulaDto formula, ECurrency currency)
        {
            var whatToDo = await GetDecisionAsync(formula, currency);

            if (whatToDo == null)
            {
                return;
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
                        Ema12 = whatToDo.Ema12.ToString(),
                        Ema26 = whatToDo.Ema26.ToString(),
                        PredictPrice = whatToDo.PredictPrice.ToString(),
                        DataLearned = formula.LimitOfDataToLearn,
                        TradingType = formula.TradingType,
                        ProfitWay = formula.ProfitWay,
                        StopLimit = formula.StopLimit,
                        StopLimitPercentageOfLoss = formula.StopLimitPercentageOfLoss,
                        StopLimitPercentageOfProfit = formula.StopLimitPercentageOfProfit
                    });
            }
        }

        public async Task<WhatToDoOutput> GetDecisionAsync(FormulaDto formula, ECurrency currency)
        {
            var data = GetRegressionData(currency, formula.IntervalToBuy, formula.TradingType, formula.LimitOfDataToLearn);

            if (data == null)
            {
                return null;
            }

            var whatToDo = await WhatToDo(formula.Strategy1, formula.InvestorProfile1, formula.ProfitWay, data);

            if (whatToDo.WhatToDo == EWhatToDo.Buy)
            {
                if (formula.Strategy2.HasValue && formula.InvestorProfile2.HasValue)
                {
                    whatToDo = await WhatToDo(formula.Strategy2.Value, formula.InvestorProfile2.Value, formula.ProfitWay, data);

                    if (whatToDo.WhatToDo == EWhatToDo.Buy
                        && formula.Strategy3.HasValue
                        && formula.InvestorProfile3.HasValue)
                    {
                        whatToDo = await WhatToDo(formula.Strategy3.Value, formula.InvestorProfile3.Value, formula.ProfitWay, data);
                    }
                }

                return whatToDo;
            }

            return whatToDo;
        }

        public RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            ETradingType tradingType,
            int limitOfDataToLearn = 120)
        {
            var sampleStock = _binanceService.GetKline($"{currency}{CryptoBotConsts.BaseCoinName}", tradingType);

            if (sampleStock == null)
            {
                return null;
            }

            var dataToLearn = _binanceService.GetData(currency, interval, tradingType, null, null, limitOfDataToLearn);

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
        public async Task AutoTraderBuyWithWalletVirtualAsync(long userId, FormulaDto formula, decimal robotInitialAmount)
        {
            var mainWallet = await _walletService.GetOrCreate(ECurrency.USDT, EWalletType.Virtual, userId, robotInitialAmount);

            if (mainWallet.Balance < formula.BalancePreserved || mainWallet.Balance <= 0)
            {
                return;
            }

            var predictions = await _predictionRepository
                .GetAll()
                .AsNoTracking()
                .Where(x =>
                    x.CreationTime > DateTime.Now.AddSeconds(-30)
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
                var bookPrice = _binanceService.GetBookPrice(userId, pair, formula.TradingType);
                var bookOrder = _binanceService.GetBookOrders(userId, pair, formula.TradingType, formula.LimitOfBookOrders);

                if (bookPrice != null
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
                            bookOrdersVerified = bidQuantity > (askQuantity + (askQuantity * formula.BookOrdersFactor / 100));
                            break;
                        case EBookOrdersAction.AskGreaterThanBid:
                            bookOrdersVerified = askQuantity > (bidQuantity + (bidQuantity * formula.BookOrdersFactor / 100));
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
                        if (mainWallet.Balance < bidQuantity * bookPrice.BestAskPrice)
                        {
                            predictionsFiltered.Add(new PredictionFilteredDto
                            {
                                Prediction = prediction,
                                BookPrice = bookPrice,
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
                priceUsdtToBuyByPredictionFiltered = (mainWallet.Balance * formula.OrderPricePerGroup) / predictionsFiltered.Count;

                var maxPriceUsdtToBuy = mainWallet.Balance * formula.MaxOrderPrice;
                
                if (priceUsdtToBuyByPredictionFiltered > maxPriceUsdtToBuy)
                {
                    priceUsdtToBuyByPredictionFiltered = maxPriceUsdtToBuy;
                }
            }
            else
            {
                priceUsdtToBuyByPredictionFiltered = formula.OrderPricePerGroup / predictionsFiltered.Count;

                if (priceUsdtToBuyByPredictionFiltered > formula.MaxOrderPrice)
                {
                    priceUsdtToBuyByPredictionFiltered = formula.MaxOrderPrice;
                }
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
            var secondsEarlier = 40;
            var now = DateTime.Now;
            var threMinutesTime = now.AddSeconds(-180 + secondsEarlier);
            var fiveMinutesTime = now.AddSeconds(-300 + secondsEarlier);
            var fifteenMinutesTime = now.AddSeconds(-900 + secondsEarlier);
            var thirtyMinutesTime = now.AddSeconds(-1800 + secondsEarlier);
            var oneHourTime = now.AddSeconds(-3600 + secondsEarlier);
            var twoHourTime = now.AddSeconds(-7200 + secondsEarlier);

            var predictionOrders = await _predictionOrderRepository
                .GetAll()
                .Include(x => x.Order)
                .Include(x => x.Prediction)
                .Where(x => x.Order.Status == EOrderStatus.Buyed)
                .ToListAsync();

            foreach (var predictionOrder in predictionOrders)
            {
                var pair = $"{predictionOrder.Order.To}{CryptoBotConsts.BaseCoinName}";
                var bookPrice = _binanceService.GetBookPrice(CryptoBotConsts.DefaultUserId, pair, predictionOrder.Prediction.TradingType);

                var amount = bookPrice.BestBidPrice * predictionOrder.Order.Amount;

                if (predictionOrder.Prediction.IntervalToSell != KlineInterval.OneMinute
                    &&
                        (
                            (predictionOrder.Prediction.IntervalToSell == KlineInterval.ThreeMinutes && predictionOrder.Prediction.CreationTime > threMinutesTime)
                            || (predictionOrder.Prediction.IntervalToSell == KlineInterval.FiveMinutes && predictionOrder.Prediction.CreationTime > fiveMinutesTime)
                            || (predictionOrder.Prediction.IntervalToSell == KlineInterval.FifteenMinutes && predictionOrder.Prediction.CreationTime > fifteenMinutesTime)
                            || (predictionOrder.Prediction.IntervalToSell == KlineInterval.ThirtyMinutes && predictionOrder.Prediction.CreationTime > thirtyMinutesTime)
                            || (predictionOrder.Prediction.IntervalToSell == KlineInterval.OneHour && predictionOrder.Prediction.CreationTime > oneHourTime)
                            || (predictionOrder.Prediction.IntervalToSell == KlineInterval.TwoHour && predictionOrder.Prediction.CreationTime > twoHourTime)
                        ))
                {
                    // TODO It just used in Virtual traders. IN real life should be a SpotLimit
                    if (predictionOrder.Prediction.StopLimit != EStopLimit.None)
                    {
                        if (predictionOrder.Prediction.StopLimit == EStopLimit.Profit
                            || predictionOrder.Prediction.StopLimit == EStopLimit.ProfitAndLoss)
                        {
                            var isProfitable = bookPrice.BestBidPrice > (predictionOrder.Order.UsdtPriceTo + predictionOrder.Order.UsdtPriceTo * predictionOrder.Prediction.StopLimitPercentageOfProfit / 100);

                            if (!isProfitable)
                            {
                                continue;
                            }
                        }

                        if (predictionOrder.Prediction.StopLimit == EStopLimit.Loss
                            || predictionOrder.Prediction.StopLimit == EStopLimit.ProfitAndLoss)
                        {
                            var isLoss = bookPrice.BestAskQuantity < (predictionOrder.Order.UsdtPriceTo + predictionOrder.Order.UsdtPriceTo * predictionOrder.Prediction.StopLimitPercentageOfLoss / 100);

                            if (!isLoss)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                var predictionOrderId = await _orderRepository.InsertAndGetIdAsync(
                    new Order
                    {
                        From = predictionOrder.Order.To,
                        To = ECurrency.USDT,
                        Status = EOrderStatus.Selled,
                        UsdtPriceFrom = bookPrice.BestBidPrice,
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
                },
                trigger =>
                {
                    trigger
                        .StartNow()
                        .WithCronSchedule("55 * * * * ?");
                });
        }

        [UnitOfWork(false)]
        public async Task StartScheduleRobots()
        {
            var robots = await _robotRepository
                .GetAll()
                .Include(x => x.Formula)
                .AsNoTracking()
                .Where(x => x.IsActive && x.Formula.IsActive)
                .ToListAsync();

            foreach (var robot in robots)
            {
                var formulaDto = _objectMapper.Map<FormulaDto>(robot.Formula);
                var robotDto = _objectMapper.Map<RobotDto>(robot);

                await ScheduleBuyVirtualTrader(robotDto, formulaDto);
            }
        }

        [UnitOfWork(false)]
        public async Task StartSchedulePredictions()
        {
            var formulas = await _formulaRepository
                .GetAll()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync();

            foreach (var formula in formulas)
            {
                var dto = _objectMapper.Map<FormulaDto>(formula);

                await ScheduleGeneratePredictions(dto);
            }
        }

        public async Task UnscheduleGeneratePredictions(long formulaId)
        {
            await _jobManager.UnscheduleAsync(new TriggerKey($"GeneratePredictionsJob-{formulaId}"));
        }

        public async Task UnscheduleBuyVirtualTrader(long robotId, long userId, long formulaId)
        {
            await _jobManager.UnscheduleAsync(new TriggerKey($"BuyVirtualTraderJob-Robot-{robotId}-User-{userId}-Formula-{formulaId}"));
        }

        public async Task ScheduleGeneratePredictions(FormulaDto formula)
        {
            try
            {
                await _jobManager.ScheduleAsync<GeneratePredictionsJob>(
                    job =>
                    {
                        job
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
                            .UsingJobData("OrderPricePerGroup", (float)formula.OrderPricePerGroup)
                            .UsingJobData("MaxOrderPrice", (float)formula.MaxOrderPrice)
                            .UsingJobData("OrderPriceType", (int)formula.OrderPriceType)
                            .UsingJobData("LimitOfBookOrders", formula.LimitOfBookOrders)
                            .UsingJobData("Description", formula.Description)
                            .UsingJobData("Currencies", formula.Currencies)
                            .UsingJobData("TradingType", (int)formula.TradingType)
                            .UsingJobData("ProfitWay", (int)formula.ProfitWay)
                            .UsingJobData("BookOrdersAction", (int)formula.BookOrdersAction)
                            .UsingJobData("BookOrdersFactor", (float)formula.BookOrdersFactor)
                            .UsingJobData("StopLimit", (int)formula.StopLimit)
                            .UsingJobData("StopLimitPercentageOfLoss", (float)formula.StopLimitPercentageOfLoss)
                            .UsingJobData("StopLimitPercentageOfProfit", (float)formula.StopLimitPercentageOfProfit);
                    },
                    trigger =>
                    {
                        trigger
                            .WithIdentity($"GeneratePredictionsJob-{formula.Id}")
                            .WithCronSchedule(BinanceHelper.GetCronExpression(formula.IntervalToBuy));
                    });
            }
            catch { }
        }

        public async Task ScheduleBuyVirtualTrader(RobotDto robot, FormulaDto formula)
        {
            var oneMinuteAfter = DateTimeOffset.Now.AddMinutes(1);

            await _jobManager.ScheduleAsync<BuyVirtualTraderJob>(
                job =>
                {
                    job
                        .UsingJobData("UserId", robot.UserId)
                        .UsingJobData("RobotInitialAmount", (float)robot.InitialAmount)
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
                        .UsingJobData("OrderPricePerGroup", (float)formula.OrderPricePerGroup)
                        .UsingJobData("MaxOrderPrice", (float)formula.MaxOrderPrice)
                        .UsingJobData("OrderPriceType", (int)formula.OrderPriceType)
                        .UsingJobData("LimitOfBookOrders", formula.LimitOfBookOrders)
                        .UsingJobData("Description", formula.Description)
                        .UsingJobData("Currencies", formula.Currencies)
                        .UsingJobData("TradingType", (int)formula.TradingType)
                        .UsingJobData("ProfitWay", (int)formula.ProfitWay)
                        .UsingJobData("BookOrdersAction", (int)formula.BookOrdersAction)
                        .UsingJobData("BookOrdersFactor", (float)formula.BookOrdersFactor)
                        .UsingJobData("StopLimit", (int)formula.StopLimit)
                        .UsingJobData("StopLimitPercentageOfLoss", (float)formula.StopLimitPercentageOfLoss)
                        .UsingJobData("StopLimitPercentageOfProfit", (float)formula.StopLimitPercentageOfProfit);
                },
                trigger =>
                {
                    trigger
                        .WithIdentity($"BuyVirtualTraderJob-Robot-{robot.Id}-User-{robot.UserId}-Formula-{formula.Id}")
                        .WithCronSchedule(BinanceHelper.GetCronExpression(formula.IntervalToBuy, 15));
                });
        }

        private async Task ScheduleGeneratePrediction(FormulaDto formula, ECurrency currency)
        {
            await _jobManager.ScheduleAsync<GeneratePredictionJob>(
                job =>
                {
                    job
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
                        .UsingJobData("OrderPricePerGroup", (float)formula.OrderPricePerGroup)
                        .UsingJobData("MaxOrderPrice", (float)formula.MaxOrderPrice)
                        .UsingJobData("OrderPriceType", (int)formula.OrderPriceType)
                        .UsingJobData("LimitOfBookOrders", formula.LimitOfBookOrders)
                        .UsingJobData("Description", formula.Description)
                        .UsingJobData("Currencies", formula.Currencies)
                        .UsingJobData("TradingType", (int)formula.TradingType)
                        .UsingJobData("ProfitWay", (int)formula.ProfitWay)
                        .UsingJobData("BookOrdersAction", (int)formula.BookOrdersAction)
                        .UsingJobData("BookOrdersFactor", (float)formula.BookOrdersFactor)
                        .UsingJobData("Currency", (int)currency)
                        .UsingJobData("StopLimit", (int)formula.StopLimit)
                        .UsingJobData("StopLimitPercentageOfLoss", (float)formula.StopLimitPercentageOfLoss)
                        .UsingJobData("StopLimitPercentageOfProfit", (float)formula.StopLimitPercentageOfProfit);
                },
                trigger =>
                {
                    trigger.StartNow();
                });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMeanReversionStrategy(RegressionDataOutput data, EProfitWay profitWay, EInvestorProfile eInvestorProfile)
        {
            var result = await _simpleMeanReversionStrategy.ShouldBuyStock(data.DataToLearn, eInvestorProfile, profitWay);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score,
                    Ema12 = result.Ema12,
                    Ema26 = result.Ema26,
                    PredictPrice = result.PredictPrice
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score,
                Ema12 = result.Ema12,
                Ema26 = result.Ema26,
                PredictPrice = result.PredictPrice
            });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMicrotrendStrategy(RegressionDataOutput data, EProfitWay profitWay, EInvestorProfile eInvestorProfile)
        {
            var result = await _simpleMicrotrendStrategy.ShouldBuyStock(data.DataToLearn, eInvestorProfile, profitWay);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score,
                    Ema12 = result.Ema12,
                    Ema26 = result.Ema26,
                    PredictPrice = result.PredictPrice
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score,
                Ema12 = result.Ema12,
                Ema26 = result.Ema26,
                PredictPrice = result.PredictPrice
            });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleRsiStrategy(RegressionDataOutput data, EProfitWay profitWay, EInvestorProfile eInvestorProfile)
        {
            var result = await _simpleStrategyRsi.ShouldBuyStock(data.DataToLearn, eInvestorProfile, profitWay);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score,
                    Ema12 = result.Ema12,
                    Ema26 = result.Ema26,
                    PredictPrice = result.PredictPrice
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score,
                Ema12 = result.Ema12,
                Ema26 = result.Ema26,
                PredictPrice = result.PredictPrice
            });
        }

        private async Task<WhatToDoOutput> WhatToDoBySimpleMlStrategy1(RegressionDataOutput data, EProfitWay profitWay, EInvestorProfile eInvestorProfile)
        {
            var result = await _simpleMlStrategy1.ShouldBuyStock(data.DataToLearn, eInvestorProfile, profitWay);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score,
                    Ema12 = result.Ema12,
                    Ema26 = result.Ema26,
                    PredictPrice = result.PredictPrice
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score,
                Ema12 = result.Ema12,
                Ema26 = result.Ema26,
                PredictPrice = result.PredictPrice
            });
        }

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy1(RegressionDataOutput data, EProfitWay profitWay, EInvestorProfile eInvestorProfile)
        {
            var result = await _normalMlStrategy1.ShouldBuyStock(data.DataToLearn, eInvestorProfile, profitWay, data.StockRightNow);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score,
                    Ema12 = result.Ema12,
                    Ema26 = result.Ema26,
                    PredictPrice = result.PredictPrice
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score,
                Ema12 = result.Ema12,
                Ema26 = result.Ema26,
                PredictPrice = result.PredictPrice
            });
        }

        private async Task<WhatToDoOutput> WhatToDoByNormalMlStrategy2(RegressionDataOutput data, EProfitWay profitWay, EInvestorProfile eInvestorProfile)
        {
            var result = await _normalMlStrategy2.ShouldBuyStock(data.DataToLearn, eInvestorProfile, profitWay, data.StockRightNow);

            if (result.Buy.HasValue && result.Buy.Value)
            {
                return await Task.FromResult(new WhatToDoOutput
                {
                    WhatToDo = EWhatToDo.Buy,
                    Score = result.Score,
                    Ema12 = result.Ema12,
                    Ema26 = result.Ema26,
                    PredictPrice = result.PredictPrice
                });
            }

            return await Task.FromResult(new WhatToDoOutput
            {
                WhatToDo = EWhatToDo.DontBuy,
                Score = result.Score,
                Ema12 = result.Ema12,
                Ema26 = result.Ema26,
                PredictPrice = result.PredictPrice
            });
        }
    }
}
