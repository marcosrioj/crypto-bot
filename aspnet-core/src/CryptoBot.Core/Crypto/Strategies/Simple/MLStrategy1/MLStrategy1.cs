using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Simple.MLStrategy1
{
    public class MLStrategy1 : DomainService, IMLStrategy1
    {
        private readonly ISettingsService _settingsService;

        public MLStrategy1(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile, EProfitWay profitWay)
        {
            var percFactor = _settingsService.GetInvestorProfileFactor(EStrategy.SimpleMlStrategy1, profitWay, eInvestorProfile);

            var modelBuilder = new ModelBuilder();
            var model = modelBuilder.BuildModel(historicalData.Select(x => new ModelInput
            {
                PriceDiffrence = (float)((x.Close - historicalData.Last().Close) / historicalData.Last().Close),
                Time = x.CloseTime
            }).ToList());
            var result = model.Predict();

            var buy = profitWay == EProfitWay.ProfitFromGain
                ? result.ForecastedPriceDiffrence[0] > percFactor
                : result.ForecastedPriceDiffrence[0] < percFactor;

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = buy,
                Score = (decimal)result.ForecastedPriceDiffrence[0]
            });
        }
    }
}
