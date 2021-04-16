using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class RegressionTestDataOutput : RegressionDataOutput
    {
        public decimal InitialWallet { get; set; }
        public List<IBinanceKline> DataToLearnAndTest { get; set; }
        public List<IBinanceKline> DataToTest { get; set; }
        public int LimitOfDataToLearnAndTest { get; set; }
        public int LimitOfDataToTest { get; set; }

        public RegressionTestDataOutput Clone()
        {
            var dataToLearnAndTest = DataToLearnAndTest.ToList();

            return new RegressionTestDataOutput
            {
                Currency = Currency,
                DataToLearnAndTest = dataToLearnAndTest,
                DataToLearn = dataToLearnAndTest.Take(LimitOfDataToLearn).ToList(),
                DataToTest = dataToLearnAndTest.Skip(LimitOfDataToLearn).Take(LimitOfDataToTest).ToList(),
                StockRightNow = StockRightNow,
                Interval = Interval,
                LimitOfDataToLearn = LimitOfDataToLearn,
                LimitOfDataToLearnAndTest = LimitOfDataToLearnAndTest,
                LimitOfDataToTest = LimitOfDataToTest,
                InitialWallet = InitialWallet
            };
        }
    }
}
