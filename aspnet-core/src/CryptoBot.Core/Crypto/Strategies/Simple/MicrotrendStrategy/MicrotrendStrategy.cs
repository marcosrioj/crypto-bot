using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy
{
    public class MicrotrendStrategy : IStrategy
    {
        public Task<bool?> ShouldBuyStock(IList<StockInput> historicalData)
        {
            var last3Values = historicalData.Skip(historicalData.Count - 3).Take(3).Select(x => x.ClosingPrice).ToList();

            //Default to hold
            var result = (bool?)null;

            if (last3Values.Count >= 3 && last3Values[0] > last3Values[1] && last3Values[1] > last3Values[2])
            {
                //Buy if we have 2 mins of increase
                result = true;
            }
            else if (last3Values.Count >= 3 && (last3Values[0] < last3Values[1] || last3Values[1] < last3Values[2]))
            {
                //Sell if any decrease in price
                result = false;
            }

            return Task.FromResult(result);
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
