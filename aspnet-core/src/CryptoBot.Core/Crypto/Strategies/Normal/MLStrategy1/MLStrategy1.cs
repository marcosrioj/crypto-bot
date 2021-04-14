using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Normal;
using CryptoBot.Crypto.Strategies.Dtos;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ML.DataOperationsCatalog;

namespace CryptoBot.Crypto.Strategies.Normal.MLStrategy1
{
    public class MLStrategy1 : DomainService, IMLStrategy1
    {
        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, IBinanceKline sampleStock)
        {
            // Cria o contexto que trabalhará com aprendizado de máquina.
            MLContext context = new MLContext();

            // Lê o arquivo e o transforma em um dataset.
            var splitData = Sanitize(context, historicalData);

            ITransformer model = Train(context, splitData.TrainSet);

            //RegressionMetrics metrics = Evaluate(context, model, splitData.TestSet);

            //SaveModel(context, model, splitData.TrainSet.Schema);

            //PrintMetrics(metrics);

            return await Task.FromResult(PredictPrice(context, model, sampleStock));
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

        private static ShouldBuyStockOutput PredictPrice(MLContext context, ITransformer model, IBinanceKline sampleStock)
        {
            PredictionEngine<StockInfo, StockInfoPrediction> predictor = context.Model
            .CreatePredictionEngine<StockInfo, StockInfoPrediction>(model);

            var actualInput = new StockInfo
            {
                Date = sampleStock.CloseTime,
                High = (float)sampleStock.High,
                Low = (float)sampleStock.Low,
                Open = (float)sampleStock.Open,
                BaseVolume = (float)sampleStock.BaseVolume,
                QuoteVolume = (float)sampleStock.QuoteVolume,
                TakerBuyBaseVolume = (float)sampleStock.TakerBuyBaseVolume,
                TakerBuyQuoteVolume = (float)sampleStock.TakerBuyQuoteVolume,
                TradeCount = sampleStock.TradeCount
            };

            StockInfoPrediction prediction = predictor.Predict(actualInput);

            return new ShouldBuyStockOutput
            {
                Buy = prediction.Close > (float)sampleStock.Close,  // Really coin price
                Score = (decimal)prediction.Close
            };

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
