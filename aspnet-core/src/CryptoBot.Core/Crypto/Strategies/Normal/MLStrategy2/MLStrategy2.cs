using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Strategies.Normal.MLStrategy2
{
    public class MLStrategy2 : DomainService, IMLStrategy2
    {
        private readonly ISettingsService _settingsService;

        public MLStrategy2(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile, EProfitWay profitWay, IBinanceKline sampleStock)
        {

            var predictor = GetPredictionEngine(out var mlContext);
            var prediction = predictor.Predict();

            string confidence = $"{prediction.ConfidenceLowerBound[0]} - {prediction.ConfidenceUpperBound[0]}";


            //predictor.CheckPoint(mlContext, "TrainedModel");

            var lastPrediction = prediction.Score[0];

            TimeSeriesPredictionEngine<CoinPrice, CoinPricePrediction> GetPredictionEngine(out MLContext mlContext)
            {
                mlContext = new MLContext(seed: 0);

                var houseData = historicalData.Select(x => new CoinPrice
                {
                    Price = (float)x.BaseVolume
                });

                // STEP 1: Common data loading configuration
                IDataView baseTrainingDataView = mlContext.Data.LoadFromEnumerable(houseData);

                var trainer = mlContext.Forecasting.ForecastBySsa(  
                    outputColumnName: nameof(CoinPricePrediction.Score),
                    inputColumnName: nameof(CoinPrice.Price),
                    windowSize: 12,
                    seriesLength: historicalData.Count,
                    trainSize: historicalData.Count,
                    horizon: 6,
                    confidenceLevel: 0.75f,
                    confidenceLowerBoundColumn: "ConfidenceLowerBound",
                    confidenceUpperBoundColumn: "ConfidenceUpperBound");

                //var model = mlContext.Forecasting.ForecastBySsa(
                //    nameof(CoinPricePrediction.Score),
                //    nameof(CoinPrice.Price),
                //    5,
                //    11, 
                //    historicalData.Count,
                //    5,
                //    confidenceLevel: 0.95f,
                //    confidenceLowerBoundColumn: "ConfidenceLowerBound",
                //    confidenceUpperBoundColumn: "ConfidenceUpperBound");

                //var trainingPipeline = dataProcessPipeline.Append(trainer);

                ITransformer trainedModel = trainer.Fit(baseTrainingDataView);

                return trainedModel.CreateTimeSeriesEngine<CoinPrice, CoinPricePrediction>(mlContext);
            }

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = lastPrediction > (float)historicalData.Last().Close,
                ScoreText = string.Join(' ', prediction.Score),
                Score = (decimal)lastPrediction // Really coin price
            }); ;
        }
    }
    public class CoinPricePrediction
    {
        [ColumnName("Score")]
        public float[] Score;
        public float[] ConfidenceLowerBound { get; set; }
        public float[] ConfidenceUpperBound { get; set; }
    }

    public class CoinPrice
    {
        [LoadColumn(0)]
        public float Price { get; set; }
    }
}
