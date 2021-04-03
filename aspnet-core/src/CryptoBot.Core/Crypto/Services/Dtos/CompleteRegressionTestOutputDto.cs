using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class CompleteRegressionTestOutputDto
    {
        public ECurrency Currency { get; set; }
        public EStrategy Strategy { get; set; }
        public decimal FinalWallet { get; set; }
        public decimal FinalTradingWallet { get; set; }
    }
}
