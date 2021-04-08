using Binance.Net.Interfaces;
using CryptoBot.Crypto.Strategies.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy
{
    public class MicrotrendStrategy : ISimpleStrategy
    {
        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData)
        {
            var last3Values = historicalData.Skip(historicalData.Count - 3).Take(3).Select(x => x.Close).ToList();

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

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = result
            });
        }
    }
}
