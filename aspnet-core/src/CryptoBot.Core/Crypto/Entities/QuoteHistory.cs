using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Crypto.Entities
{
    public class QuoteHistory : CreationAuditedEntity<long>
    {
        public ECurrency Currency { get; set; }

        [Column(TypeName = "decimal(18, 18)")]
        public decimal Price { get; set; }

        public Guid MomentReference { get; set; }
    }
}
