using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Normal;
using CryptoBot.Crypto.Dtos.Simple;
using CryptoBot.Crypto.Enums;
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

namespace CryptoBot.Crypto.Strategies.Normal.MLStrategy
{
    public class MLStrategy : INormalStrategy
    {
        public Task<bool> ShouldBuyStock(IList<IBinanceKline> historicalData, IBinanceKline actualStock)
        {
            // Cria o contexto que trabalhará com aprendizado de máquina.
            MLContext context = new MLContext();

            // Lê o arquivo e o transforma em um dataset.
            var splitData = Sanitize(context, historicalData);

            ITransformer model = Train(context, splitData.TrainSet);

            //RegressionMetrics metrics = Evaluate(context, model, splitData.TestSet);

            //SaveModel(context, model, splitData.TrainSet.Schema);

            //PrintMetrics(metrics);

            return Task.FromResult(PredictPrice(context, model, actualStock));
        }

        private static TrainTestData Sanitize(MLContext context, IList<IBinanceKline> historicalData)
        {
            var input = historicalData.Select(x => new StockInfo
            {
                Close = (float)x.Close,
                Date = x.CloseTime,
                High = (float)x.High,
                Low = (float)x.Low,
                Open = (float)x.Open,
                BaseVolume = (float)x.BaseVolume,
                QuoteVolume = (float)x.QuoteVolume,
                TakerBuyBaseVolume = (float)x.TakerBuyBaseVolume,
                TakerBuyQuoteVolume = (float)x.TakerBuyQuoteVolume,
                TradeCount = x.TradeCount

            }).ToList();

            // Lê o arquivo e o transforma em um dataset.
            IDataView dataview = context.Data.LoadFromEnumerable(input);

            // Remove as linhas que contiverem algum valor nulo.
            dataview = context.Data.FilterRowsByMissingValues(dataview, "Open",
            "High", "Low", "BaseVolume");

            // Divide o dataset em uma base de treino (70%) e uma de teste (20%).
            TrainTestData trainTestData = context.Data.TrainTestSplit(dataview, 0.2);

            return trainTestData;
        }

        private static ITransformer Train(MLContext context, IDataView trainData)
        {
            var trainer = context.Regression.Trainers.Sdca();

            string[] featureColumns = { "Open", "High", "Low", "BaseVolume", "QuoteVolume", "TakerBuyBaseVolume", "TakerBuyQuoteVolume", "TradeCount" };

            // Constroi o fluxo de transformação de dados e processamento do modelo.
            IEstimator<ITransformer> pipeline = context.Transforms
            .CopyColumns("Label", "Close")
            .Append(context.Transforms.Concatenate("Features", featureColumns))
            .Append(context.Transforms.NormalizeMinMax("Features"))
            .AppendCacheCheckpoint(context)
            .Append(trainer);

            ITransformer model = pipeline.Fit(trainData);

            return model;
        }

        private static RegressionMetrics Evaluate(MLContext context, ITransformer model,
        IDataView testSet)
        {
            IDataView predictions = model.Transform(testSet);

            RegressionMetrics metrics = context.Regression.Evaluate(predictions);

            return metrics;
        }

        //private static void SaveModel(MLContext context, ITransformer model,
        //DataViewSchema schema)
        //{
        //    if (!Directory.Exists(BasePath))
        //    {
        //        Directory.CreateDirectory(BasePath);
        //    }
        //    else
        //    {
        //        foreach (String file in Directory.EnumerateFiles(BasePath))
        //        {
        //            File.Delete(file);
        //        }
        //    }

        //    context.Model.Save(model, schema, ModelPath);
        //}

        //private static void PrintMetrics(RegressionMetrics metrics)
        //{
        //    Console.WriteLine("-------------------- MÉTRICAS --------------------");
        //    Console.WriteLine($"Mean Absolute Error: {metrics.MeanAbsoluteError}");
        //    Console.WriteLine($"Mean Squared Error: {metrics.MeanSquaredError}");
        //    Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError}");
        //    Console.WriteLine($"R Squared: {metrics.RSquared}");
        //    Console.WriteLine("--------------------------------------------------");
        //}

        private static bool PredictPrice(MLContext context, ITransformer model, IBinanceKline actualStock)
        {
            PredictionEngine<StockInfo, StockInfoPrediction> predictor = context.Model
            .CreatePredictionEngine<StockInfo, StockInfoPrediction>(model);


            var actualInput = new StockInfo
            {
                Close = 0,
                Date = actualStock.CloseTime,
                High = (float)actualStock.High,
                Low = (float)actualStock.Low,
                Open = (float)actualStock.Open,
                BaseVolume = (float)actualStock.BaseVolume,
                QuoteVolume = (float)actualStock.QuoteVolume,
                TakerBuyBaseVolume = (float)actualStock.TakerBuyBaseVolume,
                TakerBuyQuoteVolume = (float)actualStock.TakerBuyQuoteVolume,
                TradeCount = actualStock.TradeCount
            };

            StockInfoPrediction prediction = predictor.Predict(actualInput);

            return prediction.Close > (float)actualStock.Close;

            //foreach (StockInfo stock in stocks)
            //{
            //    StockInfoPrediction prediction = predictor.Predict(stock);

            //    Console.WriteLine("---------------- PREVISÃO ----------------");
            //    Console.WriteLine($"O preço previsto para a ação é de R$ {prediction.Close:0.#0}");
            //    Console.WriteLine($"O preço atual é de R$ {stock.Close:0.#0}");
            //    Console.WriteLine($"Diferença de R$ {prediction.Close - stock.Close:0.#0}");
            //    Console.WriteLine("------------------------------------------");
            //}
        }

    }
}
