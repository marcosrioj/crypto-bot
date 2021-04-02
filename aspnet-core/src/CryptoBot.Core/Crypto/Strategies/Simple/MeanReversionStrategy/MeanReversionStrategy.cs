using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBot.Crypto.Dtos.Simple;

namespace CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy
{
    public class MeanReversionStrategy : IStrategy
    {
        public Task<bool?> ShouldBuyStock(IList<StockInput> historicalData)
        {
            if (historicalData.Count > 20)
            {
                var histData = historicalData.Skip(historicalData.Count - 20).Take(20).ToList();

                var avg = histData.Select(x => x.ClosingPrice).Average();
                var diff = avg - histData.OrderByDescending(x => x.Time).First().ClosingPrice;

                return Task.FromResult<bool?>(diff >= 0);
            }

            return Task.FromResult<bool?>(null);
        }
    }
}
