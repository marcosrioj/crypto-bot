using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Crypto.Entities
{
    public class Order : CreationAuditedEntity<long>
    {
        public ECurrency From { get; set; }

        public ECurrency To { get; set; }

        public EOrderStatus Status { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal UsdtPriceFrom { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal UsdtPriceTo { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal Amount { get; set; }

        public long? OriginOrderId { get; set; }
        public Order OriginOrder { get; set; }
    }
}
