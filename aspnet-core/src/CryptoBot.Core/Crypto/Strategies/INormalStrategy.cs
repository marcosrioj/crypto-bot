using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Net.Interfaces;

namespace CryptoBot.Crypto.Strategies
{
    public interface INormalStrategy
    {
        Task<bool> ShouldBuyStock(IList<IBinanceKline> historicalData, IBinanceKline actualStock);
    }
}
