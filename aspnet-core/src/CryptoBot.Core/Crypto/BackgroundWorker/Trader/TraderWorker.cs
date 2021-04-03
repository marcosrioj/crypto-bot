using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Helpers;
using CryptoBot.Crypto.Strategies.Simple.MLStrategy;

namespace CryptoBot.Crypto.BackgroundWorker.Trader
{
    public class TraderWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ILogger<TraderWorker> _logger;
        private readonly IBinanceService _binanceService;
        private readonly ITraderTestService _traderTestService;

        public TraderWorker(
            AbpAsyncTimer timer,
            ILogger<TraderWorker> logger,
            IBinanceService binanceService,
            ITraderTestService traderTestService)
            : base(timer)
        {
            Timer.Period = 1000;
            _logger = logger;
            _binanceService = binanceService;
            _traderTestService = traderTestService;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            try
            {
                //if (DateTime.Now.Second == 0)
                //{
                //await Test2();
                await Test3();
                //}
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Worker error: TraderWorker - Message: {ex.Message}");
            }
        }

        private async Task Test2()
        {
            var interval = KlineInterval.FifteenMinutes;
            var limitOfDetails = 1000;
            var coin = ECurrency.BTC;
            var pair = $"{coin}{CryptoBotConsts.BaseCoinName}";

            var klinesResult = _binanceService.GetKlines(pair, interval, limitOfDetails);

            if (klinesResult.Success)
            {
                var input = klinesResult.Data.Select(x => new StockInput
                {
                    ClosingPrice = x.Close,
                    StockSymbol = coin.ToString(),
                    Time = x.CloseTime
                }).ToList();

                var stg = new MLStrategy();

                var input1 = input.Take(880).ToList();
                var inputForTest1 = input.Skip(880).Take(120).ToList();

                var walletPrice = 1000;
                var walletInvestingPrice = 1000;

                var futureValueCoin = inputForTest1.First();
                inputForTest1.Remove(futureValueCoin);

                LogHelper.Log($"\nTrader Worker - Coin: {coin}, InitialWallet: {walletPrice:C2}, InitialWalletInvesting: {walletInvestingPrice:C2}, Interval: {interval}", "trader_worker_test");

                await Test2Check(
                    1, futureValueCoin, walletPrice, walletInvestingPrice, stg, input1, inputForTest1);
            }
        }

        private async Task Test3()
        {
            await _traderTestService.RegressionTest(EStrategy.SimpleMlStrategy, ECurrency.BTC, KlineInterval.FifteenMinutes,
                1000, ELogLevel.FullLog, 1000);

            await _traderTestService.RegressionTest(EStrategy.SimpleMlStrategy, ECurrency.BTC, KlineInterval.FifteenMinutes,
                1000, ELogLevel.FullLog, 10000);
        }

        private async Task Test2Check(int index, StockInput futuruValueCoin, decimal walletPrice, decimal walletInvestingPrice, MLStrategy stg, List<StockInput> input1, List<StockInput> futureValues)
        {
            var result = await stg.ShouldBuyStock(input1);

            if (result.HasValue)
            {
                var actualValue = input1.Last().ClosingPrice;

                var percFuturuValueDiff = (futuruValueCoin.ClosingPrice / actualValue) - 1;

                var newWalletPrice = walletPrice * (percFuturuValueDiff + 1);

                var newWalletInvestingPrice = result.Value ? walletInvestingPrice * (percFuturuValueDiff + 1) : walletInvestingPrice;

                var resultTrade = newWalletInvestingPrice > newWalletPrice ? "winner".PadRight(6, ' ') : "loser".PadRight(6, ' ');

                string action = result.Value
                    ? "BUY".PadRight(8, ' ')
                    : "DONT BUY".PadRight(8, ' ');

                var i = index.ToString().PadLeft(5, ' ');

                var newWalletPriceStr = $"{newWalletPrice:C2}".PadLeft(9, ' ');
                var newWalletInvestingPriceStr = $"{newWalletInvestingPrice:C2}".PadLeft(9, ' ');

                var percFuturuValueDiffStr = $"{percFuturuValueDiff:P2}".PadLeft(7, ' ');

                var message =
                    $"{i} - ActualPrice: {actualValue}, FuturePrice: {futuruValueCoin.ClosingPrice}, PercDiff: {percFuturuValueDiffStr}, {action}, Result: {resultTrade}, FutureWallet: {newWalletPriceStr}, FutureWalletInvesting: {newWalletInvestingPriceStr}";

                LogHelper.Log(message, "trader_worker_test");

                if (futureValues.Count > 0)
                {
                    var futureValueCoin2 = futureValues.First();
                    futureValues.Remove(futureValueCoin2);

                    var firstValue = input1.First();
                    input1.Remove(firstValue);
                    input1.Add(futuruValueCoin);

                    await Test2Check(++index, futureValueCoin2, newWalletPrice, newWalletInvestingPrice, stg, input1, futureValues);
                }
            }
        }

    }
}
