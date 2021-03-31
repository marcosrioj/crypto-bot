using CryptoBot.Crypto.Ia.Enums;

namespace CryptoBot.Crypto.Ia.Dtos
{
    public record StrategysStock
    {
        public string Strategy { get; init; }
        public string StockSymbol { get; init; }
        public ETradingFrequency TradingFrequency { get; init; }
    }
}