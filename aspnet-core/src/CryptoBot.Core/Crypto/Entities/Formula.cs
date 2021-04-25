using Abp.Domain.Entities;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Crypto.Entities
{
    public class Formula : Entity<long>
    {
        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Currencies { get; set; }

        public EStrategy Strategy1 { get; set; }

        public EInvestorProfile InvestorProfile1 { get; set; }

        public EStrategy? Strategy2 { get; set; }

        public EInvestorProfile? InvestorProfile2 { get; set; }

        public EStrategy? Strategy3 { get; set; }

        public EInvestorProfile? InvestorProfile3 { get; set; }

        public KlineInterval IntervalToBuy { get; set; }

        public KlineInterval IntervalToSell { get; set; }

        public EBookOrdersAction BookOrdersAction { get; set; }

        [Column(TypeName = "decimal(2, 2)")]
        public decimal BookOrdersFactor { get; set; }

        public int LimitOfBookOrders { get; set; }

        public int LimitOfDataToLearn { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal BalancePreserved { get; set; }

        public EOrderPriceType OrderPriceType { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal MaxOrderPrice { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal OrderPricePerGroup { get; set; }

        public ETryToSellByMinute TryToSellByMinute { get; set; }

        [Column(TypeName = "decimal(4, 4)")]
        public decimal TryToSellByMinutePercentageOfProfit { get; set; }

        [Column(TypeName = "decimal(4, 4)")]
        public decimal TryToSellByMinutePercentageOfLoss { get; set; }

    }
}
