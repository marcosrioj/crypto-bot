using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Strategies
{
    public interface IStrategy
    {
        Task<bool?> ShouldBuyStock(IList<StockInput> historicalData);

        Task<bool?> ShouldBuyStock(IList<IBinanceKline> historicalData, ECurrency currency);
    }
}
