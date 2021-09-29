namespace CryptoBot.Crypto.Strategies.Dtos
{
    public class ShouldBuyStockOutput
    {
        public bool? Buy { get; set; }
        public decimal Score { get; set; }
        public decimal Ema12 { get; set; }
        public decimal Ema26 { get; set; }
        public decimal PredictPrice { get; set; }
    }
}
