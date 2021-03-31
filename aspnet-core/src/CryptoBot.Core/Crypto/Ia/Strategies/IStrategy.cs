using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoBot.Crypto.Ia.Dtos;

namespace CryptoBot.Crypto.Ia.Strategies
{
    public interface IStrategy
    {
        Task<bool?> ShouldBuyStock(IList<StockInput> historicalData);
    }
}
