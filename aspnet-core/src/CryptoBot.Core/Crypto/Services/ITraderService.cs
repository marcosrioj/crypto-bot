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

        Task<List<BetterCoinsTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            int limitOfDataToLearn = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<List<BetterCoinsTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
                    List<EStrategy> strategies,
                    EInvestorProfile eInvestorProfile,
                    KlineInterval interval,
                    int limitOfDataToLearn = 1000,
                    DateTime? startTime = null,
                    DateTime? endTime = null);

        Task<List<BetterCoinsTradeRightNowOutputDto>> FilterBetterCoinsToTraderRightNowAsync(
                    EStrategy strategy,
                    EInvestorProfile eInvestorProfile,
                    List<BetterCoinsTradeRightNowOutputDto> input);

        RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            int limitOfDataToLearn = 120,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<RegressionOutputDto> RegressionExec(
                    EStrategy strategy,
                    EInvestorProfile eInvestorProfile,
                    RegressionDataOutput data);
    }
}
