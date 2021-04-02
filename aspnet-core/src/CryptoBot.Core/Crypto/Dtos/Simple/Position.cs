namespace CryptoBot.Crypto.Dtos.Simple
{
    public record Position
    {
        public string StockSymbol { get; init; }
        public long NumberOfShares { get; init; }
    }
}