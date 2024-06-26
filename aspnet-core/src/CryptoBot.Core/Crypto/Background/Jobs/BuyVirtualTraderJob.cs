﻿using Abp.Dependency;
using Abp.Quartz;
using Binance.Net.Enums;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services;
using Quartz;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Background.Jobs
{
    [DisallowConcurrentExecution]
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
            var robotInitialAmount = (decimal)dataMap.GetFloat("RobotInitialAmount");
                
            var formula = new FormulaDto
            {
                Id = dataMap.GetLong("Id"),
                IntervalToBuy = (KlineInterval)dataMap.GetInt("IntervalToBuy"),
                IntervalToSell = (KlineInterval)dataMap.GetInt("IntervalToSell"),
                LimitOfDataToLearn = dataMap.GetInt("LimitOfDataToLearn"),
                Strategy1 = (EStrategy)dataMap.GetInt("Strategy1"),
                InvestorProfile1 = (EInvestorProfile)dataMap.GetInt("InvestorProfile1"),
                Strategy2 = dataMap.GetInt("Strategy2") > 0 ? (EStrategy)dataMap.GetInt("Strategy2") : null,
                InvestorProfile2 = dataMap.GetInt("InvestorProfile2") > 0 ? (EInvestorProfile)dataMap.GetInt("InvestorProfile2") : null,
                Strategy3 = dataMap.GetInt("Strategy3") > 0 ? (EStrategy)dataMap.GetInt("Strategy3") : null,
                InvestorProfile3 = dataMap.GetInt("InvestorProfile3") > 0 ? (EInvestorProfile)dataMap.GetInt("InvestorProfile3") : null,
                BalancePreserved = (decimal)dataMap.GetFloat("BalancePreserved"),
                OrderPricePerGroup = (decimal)dataMap.GetFloat("OrderPricePerGroup"),
                MaxOrderPrice = (decimal)dataMap.GetFloat("MaxOrderPrice"),
                OrderPriceType = (EOrderPriceType)dataMap.GetInt("OrderPriceType"),
                LimitOfBookOrders = dataMap.GetInt("LimitOfBookOrders"),
                Description = dataMap.GetString("Description"),
                Currencies = dataMap.GetString("Currencies"),
                TradingType = (ETradingType)dataMap.GetInt("TradingType"),
                ProfitWay = (EProfitWay)dataMap.GetInt("ProfitWay"),
                BookOrdersAction = (EBookOrdersAction)dataMap.GetInt("BookOrdersAction"),
                BookOrdersFactor = (decimal)dataMap.GetFloat("BookOrdersFactor"),
                StopLimit = (EStopLimit)dataMap.GetInt("StopLimit"),
                StopLimitPercentageOfLoss = (decimal)dataMap.GetFloat("StopLimitPercentageOfLoss"),
                StopLimitPercentageOfProfit = (decimal)dataMap.GetFloat("StopLimitPercentageOfProfit")
            };

            await _traderService.AutoTraderBuyWithWalletVirtualAsync(userId, formula, robotInitialAmount);
        }
    }
}
