using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using Microsoft.ML;
using Microsoft.ML.Data;
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

            var ema12 = Indicator.GetEma(history, 12).Last().Ema.Value;
            var ema26 = Indicator.GetEma(history, 26).Last().Ema.Value;
            var macdHistogram = Indicator.GetMacd(history, 12, 26, 9).Last().Histogram;

            PredictOutput price = await Predict(history);

            if (CryptoBotConsts.IterationNumberToBuyAgain > 0)
            {
                CryptoBotConsts.IterationNumberToBuyAgain = CryptoBotConsts.IterationNumberToBuyAgain - 1;
                return await Task.FromResult(new ShouldBuyStockOutput
                {
                    Buy = false,
                    Score = lastRsiValues.Last().Value,
                    Ema12 = ema12,
                    Ema26 = ema26,
                    PredictPrice = (decimal)price.Price
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
                        Score = lastRsiValues.Last().Value,
                        Ema12 = ema12,
                        Ema26 = ema26,
                        PredictPrice = (decimal)price.Price
                    });
                }

                previousValue = value;
                count++;
            }

            var lastRsi = lastRsiValues.Last().Value;

            var buy = lastRsi < 75 && lastRsi > 60;

            if (buy)
            {
                CryptoBotConsts.IterationNumberToBuyAgain = 5;
            }

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = buy,
                Score = lastRsi,
                Ema12 = ema12,
                Ema26 = ema26,
                PredictPrice = (decimal)price.Price
            });
        }

        private static async Task<PredictOutput> Predict(List<Quote> history)
        {
            MLContext mlContext = new MLContext();
            // 1. Import or create training data
            var houseData = history.Select(x => new PredictInput
            {
                Price = (float)x.Close,
                Size = (float)x.Volume
            });
            IDataView trainingData = mlContext.Data.LoadFromEnumerable(houseData);
            // 2. Specify data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "Size" })
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Price", maximumNumberOfIterations: 100));
            // 3. Train model
            var model = pipeline.Fit(trainingData);
            // 4. Make a prediction
            IEnumerable<Quote> historyPrec = history.Select(x => new Quote
            {
                Close = x.Close,
                Date = x.Date,
                High = x.High,
                Low = x.Low,
                Open = x.Open,
                Volume = x.Volume
            }).ToList();
            var results = await Task.FromResult(Indicator.GetVolSma(historyPrec, 20));
            var result = results.LastOrDefault();
            var size = new PredictInput() { Size = (float)result.Volume };
            var price = mlContext.Model.CreatePredictionEngine<PredictInput, PredictOutput>(model).Predict(size);
            return price;
        }
    }

    public class PredictInput
    {
        public float Size { get; set; }
        public float Price { get; set; }
    }

    public class PredictOutput
    {
        [ColumnName("Score")]
        public float Price { get; set; }
    }
}
