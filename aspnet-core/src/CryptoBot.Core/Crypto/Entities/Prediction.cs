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

        public EStrategy Strategy1 { get; set; }
        public EInvestorProfile InvestorProfile1 { get; set; }

        public EStrategy? Strategy2 { get; set; }
        public EInvestorProfile? InvestorProfile2 { get; set; }

        public EStrategy? Strategy3 { get; set; }
        public EInvestorProfile? InvestorProfile3 { get; set; }

        public KlineInterval IntervalToBuy { get; set; }
        public KlineInterval IntervalToSell { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Score { get; set; }

        public int DataLearned { get; set; }

        public ETryToSellByMinute TryToSellByMinute { get; set; }

        [Column(TypeName = "decimal(4, 4)")]
        public decimal TryToSellByMinutePercentageOfProfit { get; set; }

        [Column(TypeName = "decimal(4, 4)")]
        public decimal TryToSellByMinutePercentageOfLoss { get; set; }

        public List<PredictionOrder> Orders { get; set; }
    }
}
