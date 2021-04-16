using Binance.Net.Interfaces;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class RegressionOutputDto
    {
        public IBinanceKline ActualStock { get; set; }
        public WhatToDoOutput WhatToDo { get; set; }
    }
}
