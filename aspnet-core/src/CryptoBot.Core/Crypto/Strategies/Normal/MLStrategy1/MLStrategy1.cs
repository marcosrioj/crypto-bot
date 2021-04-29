﻿using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Dtos.Normal;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using Microsoft.ML;
using Microsoft.ML.Data;
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

        public async Task<ShouldBuyStockOutput> ShouldBuyStock(IList<IBinanceKline> historicalData, EInvestorProfile eInvestorProfile, IBinanceKline sampleStock)
        {
            // Cria o contexto que trabalhará com aprendizado de máquina.
            MLContext context = new MLContext();

            // Lê o arquivo e o transforma em um dataset.
            var splitData = Sanitize(context, historicalData);

            ITransformer model = Train(context, splitData.TrainSet);

            //RegressionMetrics metrics = Evaluate(context, model, splitData.TestSet);

            //SaveModel(context, model, splitData.TrainSet.Schema);

            //PrintMetrics(metrics);

            return await Task.FromResult(PredictPrice(context, model, sampleStock, eInvestorProfile));
        }

        private static TrainTestData Sanitize(MLContext context, IList<IBinanceKline> historicalData)
        {
            var input = historicalData.Select(x => new StockInfo
            {
                Close = (float)x.Close,
                Date = x.CloseTime,
                High = (float)x.High,
                Low = (float)x.Low

            }).ToList();

            // Lê o arquivo e o transforma em um dataset.
            IDataView dataview = context.Data.LoadFromEnumerable(input);

            // Remove as linhas que contiverem algum valor nulo.
            dataview = context.Data.FilterRowsByMissingValues(dataview, "High", "Low", "Close");

            // Divide o dataset em uma base de treino (70%) e uma de teste (20%).
            TrainTestData trainTestData = context.Data.TrainTestSplit(dataview, 0.2);

            return trainTestData;
        }

        private static ITransformer Train(MLContext context, IDataView trainData)
        {
            var trainer = context.Regression.Trainers.Sdca();

            string[] featureColumns = { "High", "Low" };

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

        private ShouldBuyStockOutput PredictPrice(MLContext context, ITransformer model, IBinanceKline sampleStock, EInvestorProfile eInvestorProfile)
        {
            PredictionEngine<StockInfo, StockInfoPrediction> predictor = context.Model
            .CreatePredictionEngine<StockInfo, StockInfoPrediction>(model);

            var actualInput = new StockInfo
            {
                Date = sampleStock.CloseTime,
                High = (float)sampleStock.High,
                Low = (float)sampleStock.Low
            };

            StockInfoPrediction prediction = predictor.Predict(actualInput);

            var percFactor = _settingsService.GetInvestorProfileFactor(EStrategy.NormalMlStrategy1, eInvestorProfile);

            var realTimeClosePrice = (float)sampleStock.Close;
            realTimeClosePrice = realTimeClosePrice * (1 + percFactor);

            return new ShouldBuyStockOutput
            {
                Buy = prediction.Close > realTimeClosePrice,  // Really coin price
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
