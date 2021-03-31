﻿using System;

namespace CryptoBot.Crypto.Ia.Dtos
{
    public record Order
    {
        public string StockSymbol { get; init; }
        public DateTime OrderPlacedTime { get; init; }
        public long SharesBought { get; init; }
        public decimal MarketPrice { get; init; }
    }
}