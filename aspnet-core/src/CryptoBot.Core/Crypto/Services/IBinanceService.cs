using Abp.Domain.Services;
using Binance.Net.Objects.Spot.MarketData;
using CryptoExchange.Net.Objects;

namespace CryptoBot.Crypto.Services
{
    public interface IBinanceService : IDomainService
    {
        WebCallResult<BinanceBookPrice> GetBookPrice(string pair);
    }
}
