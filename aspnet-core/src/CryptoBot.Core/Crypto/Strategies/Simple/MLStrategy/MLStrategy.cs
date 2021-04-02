using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBot.Crypto.Dtos.Simple;

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
    }
}
