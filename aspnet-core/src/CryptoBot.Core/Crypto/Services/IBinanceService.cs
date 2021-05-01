using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.MarketData;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services
{
    public interface IBinanceService : IDomainService
    {
        BinanceBookPrice GetBookPrice(long userId, string pair, ETradingType tradingType);

        WebCallResult<IEnumerable<IBinanceKline>> GetKlines(
            string pair,
            KlineInterval interval,
            ETradingType tradingType,
            int limit = 100,
            long? userId = null, 
            DateTime? startTime = null,
            DateTime? endTime = null);

        IBinanceKline GetKline(string pair, ETradingType tradingType, long? userId = null);

        List<IBinanceKline> GetData(
            ECurrency currency,
            KlineInterval interval,
            ETradingType tradingType,
            DateTime? startTime,
            DateTime? endTime,
            int limitOfDetails,
            long? userId = null);

        void Samples();

        WebCallResult<BinanceOrderBook> GetBookOrders(long userId, string pair, ETradingType tradingType, int? limit = null);
    }
}
