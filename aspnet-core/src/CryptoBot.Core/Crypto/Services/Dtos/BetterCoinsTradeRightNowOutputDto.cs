using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class BetterCoinsTradeRightNowOutputDto
    {
        public ECurrency Currency { get; set; }
        public WhatToDoOutput WhatToDo { get; set; }
        public RegressionDataOutput Data { get; set; }
    }
}
