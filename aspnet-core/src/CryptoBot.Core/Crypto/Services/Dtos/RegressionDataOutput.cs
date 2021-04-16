using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class RegressionDataOutput
    {
        public ECurrency Currency { get; set; }
        public List<IBinanceKline> DataToLearn { get; set; }
        public IBinanceKline StockRightNow { get; set; }
        public KlineInterval Interval { get; set; }
        public int LimitOfDataToLearn { get; set; }
    }
}
