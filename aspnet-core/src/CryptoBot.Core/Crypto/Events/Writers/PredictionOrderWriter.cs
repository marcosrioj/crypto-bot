using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Handlers;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Events.Data;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Events.Writers
{
    public class PredictionOrderWriter : IAsyncEventHandler<PredictionOrderCreatedEventData>, ITransientDependency
    {
        public readonly IRepository<PredictionOrder, long> _predictionOrderRepository;
        public readonly IRepository<Order, long> _orderRepository;

        private readonly IWalletService _walletService;
        private readonly IBinanceService _binanceService;

        public PredictionOrderWriter(
            IRepository<PredictionOrder, long> predictionOrderRepository,
            IRepository<Order, long> orderRepository,
            IWalletService walletService,
            IBinanceService binanceService)
        {
            _predictionOrderRepository = predictionOrderRepository;
            _orderRepository = orderRepository;

            _walletService = walletService;
            _binanceService = binanceService;
        }

        [UnitOfWork(false)]
        public async Task HandleEventAsync(PredictionOrderCreatedEventData eventData)
        {
            var predictionOrder = await _predictionOrderRepository
                .GetAll()
                .Include(x => x.Order)
                .Include(x => x.Prediction)
                .Where(x => x.Id == eventData.PredictionOrderId)
                .FirstAsync();

            var secondsToexecute = BinanceHelper.GetSeconds(predictionOrder.Prediction.Interval) - 10;

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            Task.Delay(secondsToexecute * 1000).ContinueWith(async (t) =>
            {
                try
                {
                    var pair = $"{predictionOrder.Order.To}{CryptoBotConsts.BaseCoinName}";
                    var bookPrice = _binanceService.GetBookPrice(pair);

                    var amount = bookPrice.Data.BestBidPrice * predictionOrder.Order.Amount;

                    var predictionOrderId = await _orderRepository.InsertAndGetIdAsync(
                        new Order
                        {
                            From = predictionOrder.Order.To,
                            To = ECurrency.USDT,
                            UsdtPriceFrom = bookPrice.Data.BestBidPrice,
                            UsdtPriceTo = 1,
                            CreatorUserId = predictionOrder.CreatorUserId,
                            Amount = amount,
                            OriginOrderId = predictionOrder.OrderId
                        });

                    await _walletService.UpdatedWalletsCustomCurrencyToUsdt(predictionOrder.CreatorUserId.Value, predictionOrder.Order.Amount, predictionOrder.Order.To, amount);

                }
                catch (System.Exception ex)
                {

                    throw;
                }
            }, cancellationToken);
        }
    }
}
