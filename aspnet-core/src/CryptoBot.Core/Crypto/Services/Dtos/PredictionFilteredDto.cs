using Binance.Net.Interfaces;
using Binance.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using CryptoBot.Crypto.Entities;
using System.Collections.Generic;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class PredictionFilteredDto
    {
        public Prediction Prediction { get; set; }
        public BinanceBookPrice BookPrice { get; internal set; }
        public IEnumerable<BinanceOrderBookEntry> AsksData { get; internal set; }
        public IEnumerable<BinanceOrderBookEntry> BidsData { get; internal set; }
    }
}
