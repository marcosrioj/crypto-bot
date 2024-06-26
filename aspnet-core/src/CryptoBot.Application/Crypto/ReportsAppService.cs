﻿using Abp.Domain.Repositories;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto
{
    public class ReportsAppService : CryptoBotAppServiceBase
    {
        private readonly IRepository<Robot, long> _robotRepository;
        private readonly IRepository<Order, long> _orderRepository;
        private readonly IRepository<Prediction, long> _predictionRepository;
        private readonly IRepository<PredictionOrder, long> _predictionOrderRepository;
        private readonly IRepository<Formula, long> _formulaRepository;
        private readonly IRepository<Wallet, long> _walletRepository;

        private readonly ITraderService _traderService;

        public ReportsAppService(
            IRepository<Robot, long> robotRepository,
            IRepository<Order, long> orderRepository,
            IRepository<Prediction, long> predictionRepository,
            IRepository<Formula, long> formulaRepository,
            IRepository<PredictionOrder, long> predictionOrderRepository,
            IRepository<Wallet, long> walletRepository,
            ITraderService traderService
            )
        {
            _robotRepository = robotRepository;
            _orderRepository = orderRepository;
            _predictionRepository = predictionRepository;
            _predictionOrderRepository = predictionOrderRepository;
            _formulaRepository = formulaRepository;
            _walletRepository = walletRepository;
            _traderService = traderService;
        }

        public async Task<object> OrdersPeformance()
        {
            var query = from originOrder in _orderRepository.GetAll().Where(o => !o.OriginOrderId.HasValue && o.Status == Enums.EOrderStatus.Selled)
                        from predictionOrder in _predictionOrderRepository.GetAll().Include(x => x.Prediction).Where(o => o.OrderId == originOrder.Id)
                        join order in _orderRepository.GetAll()
                            on originOrder.Id equals order.OriginOrderId
                        select new { order, originOrder, predictionOrder };

            var orders = await query.ToListAsync();

            var result = new Dictionary<string, dynamic>();
            var keys = orders
                .Select(x => new
                {
                    x.predictionOrder.Prediction.IntervalToBuy,
                    x.predictionOrder.Prediction.IntervalToSell
                })
                .Distinct()
                .ToList();

            foreach (var key in keys)
            {
                var keyDic = $"Buy{key.IntervalToBuy}-Sell{key.IntervalToSell}";
                dynamic itemResult = new List<dynamic>();

                var avgSuccessPerc = 0m;
                var avgFailedPerc = 0m;
                var totalSuccess = 0;
                var totalFailed = 0;
                var balance = 1000m;

                foreach (var order in orders)
                {
                    if (key.IntervalToSell == order.predictionOrder.Prediction.IntervalToSell
                        && key.IntervalToBuy == order.predictionOrder.Prediction.IntervalToBuy)
                    {
                        var perc = order.order.UsdtPriceFrom / order.originOrder.UsdtPriceTo - 1;

                        balance = order.predictionOrder.Prediction.ProfitWay == Enums.EProfitWay.ProfitFromGain
                            ? balance - (order.originOrder.Amount * order.originOrder.UsdtPriceTo) + order.order.Amount
                            : balance + (order.originOrder.Amount * order.originOrder.UsdtPriceTo) + order.order.Amount; //TODO CHECK LOGIC

                        var item = new
                        {
                            Id = order.order.Id,
                            OriginOrderId = order.order.OriginOrderId,
                            Currency = order.order.From.ToString(),
                            Perc = perc * 100,
                            CreationTime = order.originOrder.CreationTime,
                            CloseTime = order.order.CreationTime,
                            UserId = order.order.CreatorUserId,
                            balance = balance
                        };

                        itemResult.Add(item);

                        if (perc > 0)
                        {
                            ++totalSuccess;
                            avgSuccessPerc = avgSuccessPerc + perc;
                        }
                        else
                        {
                            ++totalFailed;
                            avgFailedPerc = avgFailedPerc + perc;
                        }
                    }
                }

                avgFailedPerc = totalFailed == 0 ? avgFailedPerc : avgFailedPerc / totalFailed;
                avgSuccessPerc = totalSuccess == 0 ? avgSuccessPerc : avgSuccessPerc / totalSuccess;

                dynamic itemFinalResult = new
                {
                    FinalBalance = balance,
                    TotalSuccess = totalSuccess,
                    TotalCount = totalSuccess + totalSuccess,
                    TotalFailed = totalFailed,
                    AvgSuccessPerc = $"{avgSuccessPerc:P6}",
                    AvgFailedPerc = $"{avgFailedPerc:P6}",
                    key.IntervalToBuy,
                    key.IntervalToSell,
                    Results = itemResult
                };

                result.Add(keyDic, itemFinalResult);
            }

            return result;
        }

        public async Task<object> GetPredictions(long formulaId)
        {
            var formula = await _formulaRepository.SingleAsync(x => x.Id == formulaId);
            var formulaDto = ObjectMapper.Map<FormulaDto>(formula);
            return await _traderService.GetDecisionsAsync(formulaDto);
        }
    }
}
