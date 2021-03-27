using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Entities
{
    public class QuotationHistory : CreationAuditedEntity<long>
    {
        public ECurrency Currency { get; set; }
        
        public decimal Price { get; set; }
    }
}
