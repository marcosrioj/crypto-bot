using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderService : IDomainService
    {
        Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            RegressionDataOutput data);

        RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 240,
            int limitOfDataToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<List<RegressionOutputDto>> RegressionExec(
                    EStrategy strategy,
                    EInvestorProfile eInvestorProfile,
                    RegressionDataOutput data,
                    ELogLevel logLevel = ELogLevel.NoLog);

        Task<List<BetterCoinsToTraderRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<List<BetterCoinsToTraderRightNowOutputDto>> FilterBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            List<BetterCoinsToTraderRightNowOutputDto> input);

        Task<List<BetterCoinsToTraderRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
                    List<EStrategy> strategies,
                    EInvestorProfile eInvestorProfile,
                    KlineInterval interval,
                    decimal initialWallet,
                    int limitOfDataToLearnAndTest = 1000,
                    ELogLevel logLevel = ELogLevel.NoLog,
                    DateTime? startTime = null,
                    DateTime? endTime = null);
    }
}
