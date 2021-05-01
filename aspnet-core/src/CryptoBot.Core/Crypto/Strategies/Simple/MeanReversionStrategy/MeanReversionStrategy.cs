using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy
{
    public class MeanReversionStrategy : DomainService, IMeanReversionStrategy
    {
        private readonly ISettingsService _settingsService;

        public MeanReversionStrategy(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile, EProfitWay profitWay)
        {
            var numberOfValues = (int)_settingsService.GetInvestorProfileFactor(EStrategy.SimpleMeanReversionStrategy, profitWay, eInvestorProfile);

            if (historicalData.Count > numberOfValues)
            {
                var histData = historicalData.Skip(historicalData.Count - numberOfValues).Take(numberOfValues).ToList();

                var lastPrice = histData.OrderByDescending(x => x.CloseTime).First().Close;

                var avg = histData.Select(x => x.Close).Average();
                var diff = avg - lastPrice;

                var buy = profitWay == EProfitWay.ProfitFromGain ? diff >= 0 : diff < 0;

                return await Task.FromResult(new ShouldBuyStockOutput { Buy = buy, Score = diff });
            }

            return await Task.FromResult(new ShouldBuyStockOutput());
        }
    }
}
