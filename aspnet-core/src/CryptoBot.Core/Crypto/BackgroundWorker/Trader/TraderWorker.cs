using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Binance.Net.Enums;
using CryptoBot.Crypto.BackgroundWorker.QuoteHistory;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Ia.Dtos;
using CryptoBot.Crypto.Ia.Strategies.MLStrategy;
using CryptoBot.Crypto.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.BackgroundWorker.Trader
{
    public class TraderWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ILogger<AddQuoteHistoryPerSecondWorker> _logger;
        private readonly IBinanceService _binanceService;
        private readonly IComparativeHistoricalService _comparativeHistoricalService;

        public TraderWorker(
            AbpAsyncTimer timer,
            ILogger<AddQuoteHistoryPerSecondWorker> logger,
            IComparativeHistoricalService comparativeHistoricalService,
            IBinanceService binanceService)
            : base(timer)
        {
            Timer.Period = 1000;
            _logger = logger;
            _binanceService = binanceService;
            _comparativeHistoricalService = comparativeHistoricalService;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            try
            {
                //if (DateTime.Now.Second == 0)
                //{
                    await Test2();
                //}
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Worker error: TraderWorker - Message: {ex.Message}");
            }
        }

        private async Task Test1()
        {
            var approachTrading = EApproachTrading.SimplePriceVariation;
            var interval = KlineInterval.OneMinute;
            var limitOfDetails = 120;
            var coin = ECurrency.ANKR;
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
                var result = await stg.ShouldBuyStock(input);

                if (result.HasValue)
                {
                    var message =
                        $"TraderWorker - {coin} - {DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}, Price:,{klinesResult.Data.Last().Close},Buy:,{result.Value}";
                    CustomLog(message);
                }
            }
        }

        private async Task Test2()
        {
            var approachTrading = EApproachTrading.SimplePriceVariation;
            var interval = KlineInterval.OneDay;
            var limitOfDetails = 240;
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

                var inputs = input
                    .Select((x, i) => new {Index = i, Value = x})
                    .GroupBy(x => x.Index / 120)
                    .Select(x => x.Select(y => y.Value).ToList())
                    .ToList();

                var input1 = inputs.First();
                var inputForTest1 = inputs.Last();

                var walletPrice = 1000;
                var walletInvestingPrice = 1000;

                var futureValueCoin = inputForTest1.First();
                inputForTest1.Remove(futureValueCoin);
                
                CustomLog($"\nTrader Worker - Coin: {coin}, InitialWallet: {walletPrice:C2}, InitialWalletInvesting: {walletInvestingPrice:C2}, Interval: {interval}");

                await Test2Check(
                    1 , futureValueCoin, walletPrice, walletInvestingPrice, stg, input1, inputForTest1);
            }
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

                var i = index.ToString().PadLeft(4, ' ');

                var newWalletPriceStr = $"{newWalletPrice:C2}".PadLeft(9, ' ');
                var newWalletInvestingPriceStr = $"{newWalletInvestingPrice:C2}".PadLeft(9, ' ');

                var percFuturuValueDiffStr = $"{percFuturuValueDiff:P2}".PadLeft(7, ' ');

                var message =
                    $"{i} - ActualPrice: {actualValue}, FuturePrice: {futuruValueCoin.ClosingPrice}, PercDiff: {percFuturuValueDiffStr}, {action}, Result: {resultTrade}, FutureWallet: {newWalletPriceStr}, FutureWalletInvesting: {newWalletInvestingPriceStr}";
                CustomLog(message);

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


        public void CustomLog(string message)
        {
            using (StreamWriter writer = new StreamWriter("trader_worker_test.log", true, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(message);
            }
        }
    }
}
