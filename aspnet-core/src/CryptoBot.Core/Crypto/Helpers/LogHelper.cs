using System.IO;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Helpers
{
    public static class LogHelper
    {
        public static void Log(string message, string logName)
        {
            using (StreamWriter writer = new StreamWriter($"{logName}.log", true, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(message);
            }
        }

        public static string CreateRegressionItemMessage(int index, IBinanceKline futureStock, decimal newWalletInvestingPrice,
            decimal newWalletPrice, EWhatToDo result, decimal percFuturuValueDiff, IBinanceKline actualStock)
        {
            var resultTrade = newWalletInvestingPrice > newWalletPrice ? "winner".PadRight(6, ' ') : "loser".PadRight(6, ' ');

            string action = result == EWhatToDo.Buy
                ? "BUY".PadRight(8, ' ')
                : "DONT BUY".PadRight(8, ' ');

            var i = index.ToString().PadLeft(5, ' ');

            var newWalletPriceStr = $"{newWalletPrice:C2}".PadLeft(9, ' ');
            var newWalletInvestingPriceStr = $"{newWalletInvestingPrice:C2}".PadLeft(9, ' ');   

            var percFuturuValueDiffStr = $"{percFuturuValueDiff:P2}".PadLeft(7, ' ');

            var dateStr = actualStock.CloseTime.ToString("yyyy-MM-dd HH:mm:ss K").PadLeft(21, ' ');

            return $"{i} - {dateStr} - ActualPrice: {actualStock.Close}, FuturePrice: {futureStock.Close}, PercDiff: {percFuturuValueDiffStr}, {action}, Result: {resultTrade}, FutureWallet: {newWalletPriceStr}, FutureWalletInvesting: {newWalletInvestingPriceStr}";
        }
    }
}
