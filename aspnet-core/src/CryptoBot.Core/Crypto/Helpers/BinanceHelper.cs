using Binance.Net.Enums;

namespace CryptoBot.Crypto.Helpers
{
    public static class BinanceHelper
    {
        public static int GetSeconds(KlineInterval interval)
        {
            var seconds = 0;

            switch (interval)
            {
                case KlineInterval.OneMinute:
                    seconds = 60;
                    break;
                case KlineInterval.ThreeMinutes:
                    seconds = 180;
                    break;
                case KlineInterval.FiveMinutes:
                    seconds = 300;
                    break;
                case KlineInterval.FifteenMinutes:
                    seconds = 900;
                    break;
                case KlineInterval.ThirtyMinutes:
                    seconds = 1800;
                    break;
                case KlineInterval.OneHour:
                    seconds = 3600;
                    break;
                case KlineInterval.TwoHour:
                    seconds = 7200;
                    break;
                case KlineInterval.FourHour:
                    seconds = 14400;
                    break;
                case KlineInterval.SixHour:
                    seconds = 21600;
                    break;
                case KlineInterval.EightHour:
                    seconds = 28800;
                    break;
                case KlineInterval.TwelveHour:
                    seconds = 43200;
                    break;
                case KlineInterval.OneDay:
                    seconds = 86400;
                    break;
                case KlineInterval.ThreeDay:
                    seconds = 259200;
                    break;
                case KlineInterval.OneWeek:
                    seconds = 604800;
                    break;
                case KlineInterval.OneMonth:
                    seconds = 2592000;
                    break;
            }

            return seconds;
        }
    }
}
