using Binance.Net.Interfaces;
using CryptoBot.Crypto.Strategies.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Simple.MLStrategy1
{
    public class MLStrategy1 : ISimpleStrategy
    {
        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData)
        {
            var modelBuilder = new ModelBuilder();
            var model = modelBuilder.BuildModel(historicalData.Select(x => new ModelInput
            {
                PriceDiffrence = (float)((x.Close - historicalData.Last().Close) / historicalData.Last().Close),
                Time = x.CloseTime
            }).ToList());
            var result = model.Predict();

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = result.ForecastedPriceDiffrence[0] > 0.005, // Score from ML
                Score = (decimal)result.ForecastedPriceDiffrence[0]
            });
        }
    }
}
