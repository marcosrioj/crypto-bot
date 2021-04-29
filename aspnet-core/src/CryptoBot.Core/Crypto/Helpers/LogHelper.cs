using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services.Dtos;

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

        public static string CreateRegressionItemMessage(int index, RegressionTestOutputDto item)
        {
            var resultTrade = item.TradingWallet > item.Wallet ? "winner".PadRight(6, ' ') : "loser".PadRight(6, ' ');

            string action = item.WhatToDo.WhatToDo == EWhatToDo.Buy
                ? "BUY".PadRight(8, ' ')
                : "DONT BUY".PadRight(8, ' ');

            var score = $"{item.WhatToDo.Score.ToString("N", new NumberFormatInfo() { NumberDecimalDigits = 5 })}".PadLeft(8, ' ');

            var i = index.ToString().PadLeft(3, ' ');

            var newWalletPriceStr = $"{item.Wallet:C2}".PadLeft(9, ' ');
            var newWalletInvestingPriceStr = $"{item.TradingWallet:C2}".PadLeft(9, ' ');

            var percFuturuCloseValueDiffStr = $"{item.FuturePercDiff:P2}".PadLeft(7, ' ');
            var percOpenHighDiffStr = $"{item.OpenHighFuturePercDiff:P2}".PadLeft(7, ' ');
            var percOpenLowDiffStr = $"{item.OpenLowFuturePercDiff:P2}".PadLeft(7, ' ');

            var dateStr = item.ActualStock.CloseTime.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours).ToString("yyyy-MM-dd HH:mm:ss").PadLeft(19, ' ');

            return $"{i} - {dateStr} - OpenHighDiff: {percOpenHighDiffStr}, OpenLowDiff: {percOpenLowDiffStr}, CloseDiff: {percFuturuCloseValueDiffStr}, {action} {score}, {resultTrade}, FWallet: {newWalletPriceStr}, FTWallet: {newWalletInvestingPriceStr}";
        }

        public static StringBuilder CreateBetterCoinsToTraderRightNowMessage(decimal initialWallet, KlineInterval interval, int limitOfDataToLearnAndTest, EStrategy strategy, EInvestorProfile investorProfile, IEnumerable<BetterCoinsToTestTradeRightNowOutputDto> result)
        {
            var success = 0m;
            var failed = 0m;
            var finalResult = new StringBuilder();
            var percTotal = 0m;
            var walletTotal = 0m;
            var tradingWalletTotal = 0m;

            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss K").PadLeft(21, ' ');
            finalResult.AppendLine($"GetBetterCoinsToTraderRightNow: Date: {date} - Strategy: {strategy} - InvestorProfile: {investorProfile} - Interval: {interval} - ItemsLearned: {limitOfDataToLearnAndTest}\n");

            result = result.OrderByDescending(x => x.WhatToDo.Score);

            foreach (var item in result)
            {
                if (item.FuturePercDiff > 0 && item.WhatToDo.WhatToDo == EWhatToDo.Buy
                    || item.FuturePercDiff <= 0 && item.WhatToDo.WhatToDo != EWhatToDo.Buy)
                {
                    ++success;
                }
                else
                {
                    ++failed;
                }

                var percFuturuValueDiffStr = $"{item.FuturePercDiff:P2}".PadLeft(7, ' ');

                finalResult.AppendLine($"Currency: {item.Currency.ToString().PadLeft(5, ' ')}, FuturePercDiff: {percFuturuValueDiffStr}, Score: {item.WhatToDo.Score}");

                percTotal = percTotal + item.FuturePercDiff;
                walletTotal = walletTotal + item.Wallet;
                tradingWalletTotal = tradingWalletTotal + item.TradingWallet;
            }
            var successResult = failed != 0 && success != 0 ? success / (success + failed) : 0;
            var failedResult = failed != 0 && success != 0 ? failed / (success + failed) : 0;

            percTotal = result.Count() != 0 ? percTotal / result.Count() : 0;
            var percTotalStr = $"{percTotal:P2}".PadLeft(7, ' ');
            var totalWalletStr = $"{walletTotal:C2}".PadLeft(10, ' ');
            var totalTradindWalletStr = $"{tradingWalletTotal:C2}".PadLeft(10, ' ');
            var initialWalletStr = $"{result.Count() * initialWallet:C2}".PadLeft(10, ' ');

            finalResult.AppendLine($"PercEarn: {percTotalStr}, InitialWallet: {initialWalletStr}, FinalWallet: {totalWalletStr}, FinalTradingWallet: {totalTradindWalletStr}");
            finalResult.AppendLine($"Success: {successResult:P2}({success})- Failed: {failedResult:P2}({failed})");

            return finalResult;
        }

    }
}
