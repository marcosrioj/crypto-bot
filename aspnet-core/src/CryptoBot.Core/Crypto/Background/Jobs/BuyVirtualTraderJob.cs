﻿using Abp.Dependency;
using Abp.Quartz;
using Binance.Net.Enums;
using CryptoBot.Crypto.Dtos.Services;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using Quartz;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Background.Jobs
{
    public class BuyVirtualTraderJob : JobBase, ITransientDependency
    {
        private readonly ITraderService _traderService;

        public BuyVirtualTraderJob(ITraderService traderService)
        {
            _traderService = traderService;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var userId = dataMap.GetLong("UserId");
                
            var formula = new FormulaDto
            {
                Id = dataMap.GetLong("Id"),
                Interval = (KlineInterval)dataMap.GetInt("Interval"),
                LimitOfDataToLearn = dataMap.GetInt("LimitOfDataToLearn"),
                Strategy1 = (EStrategy)dataMap.GetInt("Strategy1"),
                InvestorProfile1 = (EInvestorProfile)dataMap.GetInt("InvestorProfile1"),
                Strategy2 = dataMap.GetInt("Strategy2") > 0 ? (EStrategy)dataMap.GetInt("Strategy2") : null,
                InvestorProfile2 = dataMap.GetInt("InvestorProfile2") > 0 ? (EInvestorProfile)dataMap.GetInt("InvestorProfile2") : null,
                Strategy3 = dataMap.GetInt("Strategy3") > 0 ? (EStrategy)dataMap.GetInt("Strategy3") : null,
                InvestorProfile3 = dataMap.GetInt("InvestorProfile3") > 0 ? (EInvestorProfile)dataMap.GetInt("InvestorProfile3") : null,
            };

            await _traderService.AutoTraderBuyWithWalletVirtualAsync(userId, formula);
        }
    }
}