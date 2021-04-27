using Abp.Domain.Repositories;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Services;
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
        private readonly IRepository<Wallet, long> _walletRepository;

        public ReportsAppService(
            IRepository<Robot, long> robotRepository,
            IRepository<Order, long> orderRepository,
            IRepository<Prediction, long> predictionRepository,
            IRepository<PredictionOrder, long> predictionOrderRepository,
            IRepository<Wallet, long> walletRepository
            )
        {
            _robotRepository = robotRepository;
            _orderRepository = orderRepository;
            _predictionRepository = predictionRepository;
            _predictionOrderRepository = predictionOrderRepository;
            _walletRepository = walletRepository;
        }

        public async Task<object> OrdersPeformance()
        {
            var query = from order in _orderRepository.GetAll().Where(o => o.OriginOrderId.HasValue)
                        join originOrder in _orderRepository.GetAll()
                            on order.OriginOrderId equals originOrder.Id
                        select new { order, originOrder };

            var orders = await query.ToListAsync();

            var result = new List<dynamic>();
            var avgSuccessPerc = 0m;
            var avgFailedPerc = 0m;
            var totalSuccess = 0;
            var totalFailed = 0;
            var balance = 1000m;

            foreach (var order in orders)
            { 
                var perc = order.order.UsdtPriceFrom / order.originOrder.UsdtPriceTo - 1;

                balance = balance - (order.originOrder.Amount * order.originOrder.UsdtPriceTo) + order.order.Amount;

                var item = new
                {
                    Id = order.order.Id,
                    OriginOrderId = order.order.OriginOrderId,
                    Currency = order.order.From.ToString(),
                    Perc = perc,
                    CreationTime = order.originOrder.CreationTime,
                    CloseTime = order.order.CreationTime,
                    UserId = order.order.CreatorUserId,
                    balance = balance
                };

                result.Add(item);

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

            avgFailedPerc = avgFailedPerc / totalFailed;
            avgSuccessPerc = avgSuccessPerc / totalSuccess;

            return new
            {
                FinalBalance = balance,
                TotalSuccess = totalSuccess,
                TotalCount = result.Count,
                TotalFailed = totalFailed,
                AvgSuccessPerc = $"{avgSuccessPerc:P6}",
                AvgFailedPerc = $"{avgFailedPerc:P6}",
                Results = result.OrderByDescending(x => x.Perc).ToList()
            };
        }
    }
}
