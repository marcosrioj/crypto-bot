﻿using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Crypto.Entities
{
    public class OrderHistory : CreationAuditedEntity<long>
    {
        public ECurrency From { get; set; }

        public ECurrency To { get; set; }

        public EWhatToDo Action { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal Average { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal Executed { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal Amount { get; set; }
    }
}
