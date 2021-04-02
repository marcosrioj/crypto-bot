using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Dtos.Simple
{
    public record StrategysStock
    {
        public string Strategy { get; init; }
        public string StockSymbol { get; init; }
        public ETradingFrequency TradingFrequency { get; init; }
    }
}