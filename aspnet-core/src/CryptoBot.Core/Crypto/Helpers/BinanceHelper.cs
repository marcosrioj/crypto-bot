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

        public static string GetCronExpression(KlineInterval interval, int seconds = 0)
        {
            var cronExpression = string.Empty;

            switch (interval)
            {
                case KlineInterval.OneMinute:
                    cronExpression = $"{seconds} * * * * ?";
                    break;
                case KlineInterval.ThreeMinutes:
                    cronExpression = $"{seconds} */3 * * * ?";
                    break;
                case KlineInterval.FiveMinutes:
                    cronExpression = $"{seconds} */5 * * * ?";
                    break;
                case KlineInterval.FifteenMinutes:
                    cronExpression = $"{seconds} */15 * * * ?";
                    break;
                case KlineInterval.ThirtyMinutes:
                    cronExpression = $"{seconds} */30 * * * ?";
                    break;
                case KlineInterval.OneHour:
                    cronExpression = $"{seconds} 0 * * * ?";
                    break;
                case KlineInterval.TwoHour:
                    cronExpression = $"{seconds} 0 */2 * * ?";
                    break;
                case KlineInterval.FourHour:
                    cronExpression = $"{seconds} 0 */4 * * ?";
                    break;
                case KlineInterval.SixHour:
                    cronExpression = $"{seconds} 0 */6 * * ?";
                    break;
                case KlineInterval.EightHour:
                    cronExpression = $"{seconds} 0 */8 * * ?";
                    break;
                case KlineInterval.TwelveHour:
                    cronExpression = $"{seconds} 0 */12 * * ?";
                    break;
                case KlineInterval.OneDay:
                    cronExpression = $"{seconds} 0 0 * * ?";
                    break;
                case KlineInterval.ThreeDay:
                    cronExpression = $"{seconds} 0 0 */3 * ?";
                    break;
                case KlineInterval.OneWeek:
                    cronExpression = $"{seconds} 0 0 */7 * ?";
                    break;
                case KlineInterval.OneMonth:
                    cronExpression = $"{seconds} 0 0 0 * ?";
                    break;
            }

            return cronExpression;
        }
    }
}
