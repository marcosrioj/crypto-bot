using Microsoft.ML.Data;

namespace CryptoBot.Crypto.Dtos.Normal
{
    public class StockInfoPrediction
    {
        [ColumnName("Score")]
        public float Close { get; set; }
    }
}
