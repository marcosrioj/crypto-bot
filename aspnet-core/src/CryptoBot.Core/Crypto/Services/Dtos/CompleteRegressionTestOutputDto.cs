using System.Collections.Generic;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class CompleteRegressionTestOutputDto
    {
        public ECurrency Currency { get; set; }
        public EStrategy Strategy { get; set; }

        public List<RegressionTestOutputDto> Results { get; set; }
    }
}
