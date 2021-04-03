using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy
{
    public class MeanReversionStrategy : ISimpleStrategy
    {
        public Task<bool?> ShouldBuyStock(IList<IBinanceKline> historicalData)
        {
            if (historicalData.Count > 20)
            {
                var histData = historicalData.Skip(historicalData.Count - 20).Take(20).ToList();

                var avg = histData.Select(x => x.Close).Average();
                var diff = avg - histData.OrderByDescending(x => x.CloseTime).First().Close;

                return Task.FromResult<bool?>(diff >= 0);
            }

            return Task.FromResult<bool?>(null);
        }
    }
}
