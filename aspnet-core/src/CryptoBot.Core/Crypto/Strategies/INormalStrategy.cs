using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Strategies.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies
{
    public interface INormalStrategy
    {
        Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile, EProfitWay profitWay, IBinanceKline actualStock);
    }
}
