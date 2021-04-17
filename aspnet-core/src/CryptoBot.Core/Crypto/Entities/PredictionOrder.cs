using Abp.Domain.Entities.Auditing;

namespace CryptoBot.Crypto.Entities
{
    public class PredictionOrder : CreationAuditedEntity<long>
    {
        public long OrderId { get; set; }
        public Order Order { get; set; }

        public long PredictionId { get; set; }
        public Prediction Prediction { get; set; }
    }
}
