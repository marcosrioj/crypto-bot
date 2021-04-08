using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CryptoBot.Crypto.Strategies.Normal.MLStrategy2
{
    public class MLStrategy2 : INormalStrategy
    {
        public async Task<bool> ShouldBuyStock(IList<IBinanceKline> historicalData, IBinanceKline sampleStock)
        {
            MLContext mlContext = new MLContext();

            // 1. Import or create training data
            var houseData = historicalData.Select(x => new Input
            {
                Price = (float) x.Close,
                Size = (float) x.BaseVolume
            });

            IDataView trainingData = mlContext.Data.LoadFromEnumerable(houseData);

            // 2. Specify data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "Size" })
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Price", maximumNumberOfIterations: 100));

            // 3. Train model
            var model = pipeline.Fit(trainingData);

            // 4. Make a prediction
            var size = new Input() { Size = houseData.Last().Size };
            var price = mlContext.Model.CreatePredictionEngine<Input, Prediction>(model).Predict(size);

            if (price.Price > (float)sampleStock.Close)
            {

            }

            return await Task.FromResult(price.Price > 0.2);

            // Predicted price for size: 2500 sq ft= $261.98k
        }
    }

    public class Input
    {
        public float Size { get; set; }
        public float Price { get; set; }
    }

    public class Prediction
    {
        [ColumnName("Score")]
        public float Price { get; set; }
    }
}
