namespace CryptoBot.Crypto.Ia.Dtos
{
    public record Position
    {
        public string StockSymbol { get; init; }
        public long NumberOfShares { get; init; }
    }
}