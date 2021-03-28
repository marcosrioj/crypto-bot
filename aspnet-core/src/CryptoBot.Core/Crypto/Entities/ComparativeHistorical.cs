using Abp.Domain.Entities.Auditing;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;

namespace CryptoBot.Crypto.Entities
{
    public class ComparativeHistorical : CreationAuditedEntity<long>
    {
        public Guid GroupId { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public List<ComparativeHistoricalDetail> Details { get; set; }
    }
}
