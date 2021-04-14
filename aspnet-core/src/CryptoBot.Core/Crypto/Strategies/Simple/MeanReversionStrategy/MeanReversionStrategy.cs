using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Strategies.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy
{
    public class MeanReversionStrategy : DomainService, IMeanReversionStrategy
    {
        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData)
        {
            if (historicalData.Count > 20)
            {
                var histData = historicalData.Skip(historicalData.Count - 20).Take(20).ToList();

                var avg = histData.Select(x => x.Close).Average();
                var diff = avg - histData.OrderByDescending(x => x.CloseTime).First().Close;

                return await Task.FromResult(new ShouldBuyStockOutput { Buy = diff >= 0 });
            }

            return await Task.FromResult(new ShouldBuyStockOutput());
        }
    }
}
