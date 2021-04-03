using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;

namespace CryptoBot.Crypto.Strategies.Simple.MLStrategy1
{
    public class MLStrategy1 : ISimpleStrategy
    {
        public Task<bool?> ShouldBuyStock(IList<IBinanceKline> historicalData)
        {
            var modelBuilder = new ModelBuilder();
            var model = modelBuilder.BuildModel(historicalData.Select(x => new ModelInput
            {
                PriceDiffrence = (float)((x.Close - historicalData.Last().Close) / historicalData.Last().Close),
                Time = x.CloseTime
            }).ToList());
            var result = model.Predict();

            return Task.FromResult((bool?)(result.ForecastedPriceDiffrence[0] > 0));
        }
    }
}
