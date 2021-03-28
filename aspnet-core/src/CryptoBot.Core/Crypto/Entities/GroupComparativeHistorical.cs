using Abp.Domain.Entities.Auditing;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using System;

namespace CryptoBot.Crypto.Entities
{
    public class GroupComparativeHistorical : CreationAuditedEntity<Guid>
    {
        public EApproachTrading ApproachTrading { get; set; }

        public ECurrency Currency { get; set; }

        public KlineInterval Interval { get; set; }

        public int LimitOfDetails { get; set; }
    }
}
