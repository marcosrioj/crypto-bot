using Microsoft.ML.Data;
using System;

namespace CryptoBot.Crypto.Dtos.Normal
{
    public class StockInfo
    {
        [LoadColumn(0)]
        public DateTime Date { get; set; }

        [LoadColumn(1)]
        public float High { get; set; }

        [LoadColumn(2)]
        public float Low { get; set; }

        [LoadColumn(3)]
        public float Close { get; set; }
    }
}
