using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Binance.Net.Enums;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using System.ComponentModel.DataAnnotations;

namespace CryptoBot.Crypto.Dtos.Services
{
    [AutoMap(typeof(Formula))]
    public class FormulaDto : EntityDto<long>
    {
        public bool IsActive { get; set; }

        [Required]
        public EStrategy Strategy1 { get; set; }

        [Required]
        public EInvestorProfile InvestorProfile1 { get; set; }

        public EStrategy? Strategy2 { get; set; }
        public EInvestorProfile? InvestorProfile2 { get; set; }

        public EStrategy? Strategy3 { get; set; }
        public EInvestorProfile? InvestorProfile3 { get; set; }

        [Required]
        public KlineInterval IntervalToBuy { get; set; }

        [Required]
        public KlineInterval IntervalToSell { get; set; }

        [Required]
        public EBookOrdersAction BookOrdersAction { get; set; }

        [Required]
        public decimal BookOrdersFactor { get; set; }

        [Required]
        public int LimitOfDataToLearn { get; set; }

    }
}
