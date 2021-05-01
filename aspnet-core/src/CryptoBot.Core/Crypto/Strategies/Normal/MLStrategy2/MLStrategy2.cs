using Abp.Domain.Services;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Strategies.Dtos;
using Microsoft.ML;
using Microsoft.ML.Data;
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
            var percFactor = _settingsService.GetInvestorProfileFactor(Enums.EStrategy.NormalMlStrategy2, profitWay, eInvestorProfile);

            MLContext mlContext = new MLContext();

            // 1. Import or create training data
            var houseData = historicalData.Select(x => new Input
            {
                Price = (float)x.Close,
                Size = (float)x.BaseVolume
            });

            IDataView trainingData = mlContext.Data.LoadFromEnumerable(houseData);

            // 2. Specify data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "Size" })
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Price", maximumNumberOfIterations: 100));

            // 3. Train model
            var model = pipeline.Fit(trainingData);

            // 4. Make a prediction
            var size = new Input() { Size = houseData.Last().Size };
            var price = mlContext.Model.CreatePredictionEngine<Input, PredictionMl2>(model).Predict(size);

            var finalPrice = (float)sampleStock.Close * (1 + percFactor);

            var buy = profitWay == EProfitWay.ProfitFromGain ? price.Price > (float)sampleStock.Close : price.Price <= (float)sampleStock.Close;

            return await Task.FromResult(new ShouldBuyStockOutput
            {
                Buy = buy,
                Score = (decimal)price.Price // Really coin price
            });
        }
    }

    public class Input
    {
        public float Size { get; set; }
        public float Price { get; set; }
    }

    public class PredictionMl2
    {
        [ColumnName("Score")]
        public float Price { get; set; }
    }
}
