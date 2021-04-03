using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Net.Interfaces;

namespace CryptoBot.Crypto.Strategies
{
    public interface ISimpleStrategy
    {
        Task<bool?> ShouldBuyStock(IList<IBinanceKline> historicalData);
    }
}
