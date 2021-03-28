﻿using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.MarketData;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;

namespace CryptoBot.Crypto.Services
{
    public interface IBinanceService : IDomainService
    {
        WebCallResult<BinanceBookPrice> GetBookPrice(string pair);

        WebCallResult<IEnumerable<IBinanceKline>> GetKlines(string pair, KlineInterval interval, int limit = 100, DateTime? startTime = null, DateTime? endTime = null);
    }
}