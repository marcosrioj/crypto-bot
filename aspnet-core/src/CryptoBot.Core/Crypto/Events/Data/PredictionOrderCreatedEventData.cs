using Abp.Events.Bus;

namespace CryptoBot.Crypto.Events.Data
{
    public class PredictionOrderCreatedEventData : EventData
    {
        public long PredictionOrderId { get; set; }
    }
}
