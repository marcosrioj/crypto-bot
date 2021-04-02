using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoBot.Crypto.Dtos.Simple;

namespace CryptoBot.Crypto.Strategies
{
    public interface IStrategy
    {
        Task<bool?> ShouldBuyStock(IList<StockInput> historicalData);
    }
}
