using System;

namespace CryptoBot.Crypto.Ia.Dtos
{
    public record StockInput
    {
        public decimal ClosingPrice { get; init; }
        public string StockSymbol { get; init; }
        public DateTime Time { get; init; }
    }
}
