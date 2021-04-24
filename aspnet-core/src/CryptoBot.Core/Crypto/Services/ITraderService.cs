﻿using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderService : IDomainService
    {
        Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionDataOutput data);

        Task GenerateBetterPredictionsAsync(FormulaDto formula);

        Task GenerateBetterPredictionAsync(FormulaDto formula, ECurrency currency);

        RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            int limitOfDataToLearn = 120);

        Task AutoTraderBuyWithWalletVirtualAsync(long userId, FormulaDto formula);

        Task AutoTraderSellWithWalletVirtualAsync();

        Task UnscheduleGeneratePredictions(long formulaId);

        Task UnscheduleBuyVirtualTrader(long userId, long formulaId);

        Task ScheduleGeneratePredictions(FormulaDto formula);

        Task ScheduleBuyVirtualTrader(long userId, FormulaDto formula);

        Task StartScheduleFormulas();

        Task ScheduleAutoTraderSellWithWalletVirtualAsync();
    }
}
