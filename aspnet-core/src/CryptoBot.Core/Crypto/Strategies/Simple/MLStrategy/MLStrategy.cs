using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Strategies.Simple.MLStrategy
{
    public class MLStrategy : IStrategy
    {
        public Task<bool?> ShouldBuyStock(IList<StockInput> historicalData)
        {
            var modelBuilder = new ModelBuilder();
            var model = modelBuilder.BuildModel(historicalData.Select(x => new ModelInput
            {
                PriceDiffrence = (float)((x.ClosingPrice - historicalData.Last().ClosingPrice) / historicalData.Last().ClosingPrice),
                Time = x.Time
            }).ToList());
            var result = model.Predict();

            return Task.FromResult((bool?)(result.ForecastedPriceDiffrence[0] > 0));
        }

        public async Task<bool?> ShouldBuyStock(IList<IBinanceKline> historicalData, ECurrency currency)
        {
            var customHistoricalData = historicalData.Select(x => new StockInput
            {
                ClosingPrice = x.Close,
                StockSymbol = currency.ToString(),
                Time = x.CloseTime
            }).ToList();

            return await ShouldBuyStock(customHistoricalData);
        }
    }
}
