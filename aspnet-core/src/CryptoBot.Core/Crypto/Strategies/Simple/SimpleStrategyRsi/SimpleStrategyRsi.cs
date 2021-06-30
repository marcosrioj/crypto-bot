using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using Skender.Stock.Indicators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy
{
    public class SimpleRsiStrategy : DomainService, ISimpleRsiStrategy
    {
        private readonly ISettingsService _settingsService;

        public SimpleRsiStrategy(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile, EProfitWay profitWay)
        {
            var rsiHistoryNecessaryCount = 14 * 10 * 2;

            if (historicalData.Count < rsiHistoryNecessaryCount)
            {
                return await Task.FromResult(new ShouldBuyStockOutput
                {
                    Buy = false
                });
            }

            List<Quote> history = historicalData
                .Skip(historicalData.Count() - rsiHistoryNecessaryCount)
                .Take(rsiHistoryNecessaryCount)
                .Select(x => new Quote
                {
                    Close = x.Close,
                    Date = x.CloseTime,
                    High = x.High,
                    Low = x.Low,
                    Open = x.Open,
                    Volume = x.BaseVolume
                })
                .ToList();

            var rsi = Indicator.GetRsi(history, 14);
            var lastRsiValues = rsi
                .Skip(rsi.Count() - 5)
                .Take(5)
                .Select(x => x.Rsi)
                .ToList();

            if (CryptoBotConsts.IterationNumberToBuyAgain > 0)
            {
                CryptoBotConsts.IterationNumberToBuyAgain = CryptoBotConsts.IterationNumberToBuyAgain - 1;
                return await Task.FromResult(new ShouldBuyStockOutput
                {
                    Buy = false,
                    Score = lastRsiValues.Last().Value
                });
            }

            decimal? previousValue = null;
            int count = 1;
            foreach (var value in lastRsiValues)
            {
                if (previousValue == null)
                {
                    previousValue = value;
                    continue;
                }

                if ((profitWay == EProfitWay.ProfitFromGain && value > previousValue)
                    || (profitWay == EProfitWay.ProfitFromLoss && value < previousValue))
                {
                    return await Task.FromResult(new ShouldBuyStockOutput
                    {
                        Buy = false,
                        Score = lastRsiValues.Last().Value
                    });
                }

                previousValue = value;
                count++;
            }

            var lastRsi = lastRsiValues.Last().Value;

            var buy = lastRsi < 40;

            if (buy)
            {
                CryptoBotConsts.IterationNumberToBuyAgain = 5;
            }

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = lastRsi < 40,
                Score = lastRsi
            });
        }
    }
}
