using Abp.Domain.Entities.Auditing;
using System;

namespace CryptoBot.Crypto.Entities
{
    public class GroupComparativeHistorical : CreationAuditedEntity<Guid>
    {
        public long UserId { get; set; }
    }
}
