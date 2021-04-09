using CryptoBot.Crypto.Enums;
using System.Collections.Generic;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class CompleteRegressionTestOutputDto
    {
        public ECurrency Currency { get; set; }
        public EStrategy Strategy { get; set; }

        public List<RegressionOutputDto> Results { get; set; }
    }
}
