using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Entities
{
    public class OrderHistory : CreationAuditedEntity<long>
    {
        public ECurrency From { get; set; }
        public ECurrency To { get; set; }
        public EOrderAction Action { get; set; }
        public decimal Average { get; set; }
        public decimal Price { get; set; }
        public decimal Executed { get; set; }
        public decimal Amount { get; set; }
    }
}
