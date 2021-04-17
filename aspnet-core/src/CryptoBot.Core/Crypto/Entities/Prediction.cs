using Abp.Domain.Entities.Auditing;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Crypto.Entities
{
    public class Prediction : CreationAuditedEntity<long>
    {
        public ECurrency Currency { get; set; }

        public EWhatToDo WhatToDo { get; set; }

        public EStrategy? Strategy { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string Strategies { get; set; }

        public EInvestorProfile InvestorProfile { get; set; }

        public EPredictionType Type { get; set; }

        public KlineInterval Interval { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal Score { get; set; }

        public int DataLearned { get; set; }

        public List<PredictionOrder> Orders { get; set; }
    }
}
