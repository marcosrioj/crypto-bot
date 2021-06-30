using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Normal;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using Microsoft.ML;
using Microsoft.ML.Data;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ML.DataOperationsCatalog;

namespace CryptoBot.Crypto.Strategies.Normal.MLStrategy1
{
    [Obsolete("O sampleStock stock é usado para prever o preco, mas nesse modelo ainda não funciona porque o período é customizável e adicionar toda as oções dessa estratégia não dá um valor real")]
    public class MLStrategy1 : DomainService, IMLStrategy1
    {
        private readonly ISettingsService _settingsService;

        public MLStrategy1(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile, EProfitWay profitWay, IBinanceKline sampleStock)
        {
            IEnumerable<Quote> history = historicalData.Select(x => new Quote
            {
                Close = x.Close,
                Date = x.CloseTime,
                High = x.High,
                Low = x.Low,
                Open = x.Open,
                Volume = x.BaseVolume
            }).ToList();

            IEnumerable<SmaResult> results = await Task.FromResult(Indicator.GetSma(history, history.Count()));

            SmaResult result = results.LastOrDefault();
            var price = result.Sma;

            var percFactor = (decimal)_settingsService.GetInvestorProfileFactor(EStrategy.NormalMlStrategy1, profitWay, eInvestorProfile);

            var realTimeClosePrice = price;
            realTimeClosePrice = realTimeClosePrice * (1 + percFactor);

            var buy = profitWay == EProfitWay.ProfitFromGain ? price > realTimeClosePrice : price <= realTimeClosePrice;

            return new ShouldBuyStockOutput
            {
                Buy = buy,
                Score = price.Value // Really coin price
            };
        }
    }
}
