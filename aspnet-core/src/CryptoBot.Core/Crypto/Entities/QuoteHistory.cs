using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;
using System;

namespace CryptoBot.Crypto.Entities
{
    public class QuoteHistory : CreationAuditedEntity<long>
    {
        public ECurrency Currency { get; set; }
        
        public decimal Price { get; set; }

        public Guid MomentReference { get; set; }
    }
}
