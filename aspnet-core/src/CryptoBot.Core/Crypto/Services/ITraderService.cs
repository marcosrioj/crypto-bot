using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderService : IDomainService
    {
        Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            EProfitWay profitWay,
            RegressionDataOutput data);

        Task GenerateBetterPredictionsAsync(FormulaDto formula);

        Task GenerateBetterPredictionAsync(FormulaDto formula, ECurrency currency);

        Task<WhatToDoOutput> GetDecisionAsync(FormulaDto formula, ECurrency currency);

        RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            ETradingType tradingType,
            int limitOfDataToLearn = 120);

        Task<List<GetDecisionsOutputDto>> GetDecisionsAsync(FormulaDto formula);

        Task AutoTraderBuyWithWalletVirtualAsync(long userId, FormulaDto formula, decimal robotInitialAmount);

        Task AutoTraderSellWithWalletVirtualAsync();

        Task UnscheduleGeneratePredictions(long formulaId);

        Task UnscheduleBuyVirtualTrader(long robotId, long userId, long formulaId);

        Task ScheduleGeneratePredictions(FormulaDto formula);

        Task ScheduleBuyVirtualTrader(RobotDto robot, FormulaDto formula);

        Task StartScheduleRobots();

        Task StartSchedulePredictions();

        Task ScheduleAutoTraderSellWithWalletVirtualAsync();
    }
}
