using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Simple.MicrotrendStrategy
{
    public class MicrotrendStrategy : DomainService, IMicrotrendStrategy
    {
        private readonly ISettingsService _settingsService;

        public MicrotrendStrategy(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile)
        {
            var numberOfTest = (int)_settingsService.GetInvestorProfileFactor(Enums.EStrategy.SimpleMicrotrendStrategy, eInvestorProfile);

            var lastValues = historicalData.Skip(historicalData.Count - numberOfTest).Take(numberOfTest).Select(x => x.Close).ToList();

            if (lastValues.Count < numberOfTest)
            {
                return await Task.FromResult(new ShouldBuyStockOutput
                {
                    Buy = false
                });
            }

            decimal? previousValue = null;
            int count = 1; 
            foreach (var value in lastValues)
            {
                if (previousValue == null)
                {
                    previousValue = value;
                    continue;
                }

                if (value > previousValue)
                {
                    return await Task.FromResult(new ShouldBuyStockOutput
                    {
                        Buy = false,
                        Score = count
                    });
                }

                previousValue = value;
                count++;
            }

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = true,
                Score = count
            });
        }
    }
}
