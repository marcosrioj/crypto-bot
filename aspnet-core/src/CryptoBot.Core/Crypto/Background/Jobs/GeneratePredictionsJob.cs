using Abp.Dependency;
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
    public class GeneratePredictionsJob : JobBase, ITransientDependency
    {
        private readonly ITraderService _traderService;

        public GeneratePredictionsJob(ITraderService traderService)
        {
            _traderService = traderService;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.JobDetail.JobDataMap;

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
                OrderPrice = (decimal)dataMap.GetFloat("OrderPrice"),
                OrderPriceType = (EOrderPriceType)dataMap.GetInt("OrderPriceType"),
                LimitOfBookOrders = dataMap.GetInt("LimitOfBookOrders"),
                Description = dataMap.GetString("Description"),
                Currencies = dataMap.GetString("Currencies"),
                BookOrdersAction = (EBookOrdersAction)dataMap.GetInt("BookOrdersAction"),
                BookOrdersFactor = (decimal)dataMap.GetFloat("BookOrdersFactor"),
                TryToSellByMinute = dataMap.GetBoolean("TryToSellByMinute"),
                TryToSellByMinutePercentage = (decimal)dataMap.GetFloat("TryToSellByMinutePercentage"),
                BalancePreserved = (decimal)dataMap.GetFloat("BalancePreserved")
            };

            await _traderService.GenerateBetterPredictionsAsync(formula);
        }
    }
}
