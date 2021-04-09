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
        public decimal InitialWallet { get; set; }
        public List<IBinanceKline> DataToLearnAndTest { get; set; }
        public List<IBinanceKline> DataToLearn { get; set; }
        public List<IBinanceKline> DataToTest { get; set; }
        public IBinanceKline SampleStockToTest { get; set; }
        public KlineInterval Interval { get; set; }
        public int LimitOfDataToLearn { get; set; }
        public int LimitOfDataToLearnAndTest { get; set; }
        public int LimitOfDataToTest { get; set; }

        public RegressionDataOutput Clone()
        {
            var dataToLearnAndTest = DataToLearnAndTest.ToList();

            return new RegressionDataOutput
            {
                Currency = Currency,
                DataToLearnAndTest = dataToLearnAndTest,
                DataToLearn = dataToLearnAndTest.Take(LimitOfDataToLearn).ToList(),
                DataToTest = dataToLearnAndTest.Skip(LimitOfDataToLearn).Take(LimitOfDataToTest).ToList(),
                SampleStockToTest = SampleStockToTest,
                Interval = Interval,
                LimitOfDataToLearn = LimitOfDataToLearn,
                LimitOfDataToLearnAndTest = LimitOfDataToLearnAndTest,
                LimitOfDataToTest = LimitOfDataToTest,
                InitialWallet = InitialWallet
            };
        }
    }
}
