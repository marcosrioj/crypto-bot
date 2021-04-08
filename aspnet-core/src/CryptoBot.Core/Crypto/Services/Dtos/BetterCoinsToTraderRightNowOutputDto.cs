using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class BetterCoinsToTraderRightNowOutputDto
    {
        public ECurrency Currency { get; set; }
        public decimal Wallet { get; set; }
        public decimal TradingWallet { get; set; }
        public IBinanceKline ActualStock { get; set; }
        public IBinanceKline FutureStock { get; set; }
        public WhatToDoOutput WhatToDo { get; set; }
        public decimal FuturePercDiff { get; set; }
    }
}
