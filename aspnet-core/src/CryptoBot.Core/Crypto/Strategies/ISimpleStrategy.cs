using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Strategies.Dtos;

namespace CryptoBot.Crypto.Strategies
{
    public interface ISimpleStrategy
    {
        Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData);
    }
}
