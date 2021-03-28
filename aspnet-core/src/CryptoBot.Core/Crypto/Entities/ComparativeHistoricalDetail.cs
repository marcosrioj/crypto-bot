using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Crypto.Entities
{
    public class ComparativeHistoricalDetail : Entity<long>
    {
        [Column(TypeName = "decimal(8, 8)")]
        public decimal PercentageGainFromPreviousDetail { get; set; }

        [Column(TypeName = "decimal(8, 8)")]
        public decimal PercentageGainFromFirstDetail { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal Amount { get; set; }

        public DateTime Moment { get; set; }

        public long ComparativeHistoricalId { get; set; }
        public ComparativeHistorical ComparativeHistorical { get; set; }
    }
}
