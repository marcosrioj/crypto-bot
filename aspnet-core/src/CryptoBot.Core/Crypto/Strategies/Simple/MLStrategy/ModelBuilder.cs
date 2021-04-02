using System;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace CryptoBot.Crypto.Strategies.Simple.MLStrategy
{
    public class ModelBuilder
    {
        private const int ConfigWindowSize = 30;
        private const int ConfigSeriesLength = 120;
        private const int ConfigHorizon = 30;
        private const float ConfigConfidenceLevel = 0.95f;

        public TimeSeriesPredictionEngine<ModelInput, ModelOutput> BuildModel(IList<ModelInput> inputs)
        {
            //Create model
            var mlContext = new MLContext();
            var trainDataVeiw = mlContext.Data.LoadFromEnumerable(inputs);

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedPriceDiffrence",
                inputColumnName: "PriceDiffrence",
                windowSize: ConfigWindowSize,
                seriesLength: ConfigSeriesLength,
                trainSize: inputs.Count,
                horizon: ConfigHorizon,
                confidenceLevel: ConfigConfidenceLevel,
                confidenceLowerBoundColumn: "LowerBoundPriceDiffrence",
                confidenceUpperBoundColumn: "UpperBoundPriceDiffrence");

            var forecaster = forecastingPipeline.Fit(trainDataVeiw);
            return forecaster.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
        }
    }

    public record ModelInput
    {
        public float PriceDiffrence { get; init; }
        public DateTime Time { get; init; }
    }

    public record ModelOutput
    {
        public float[] ForecastedPriceDiffrence { get; init; }
        public float[] LowerBoundPriceDiffrence { get; init; }
        public float[] UpperBoundPriceDiffrence { get; init; }
    }
}
