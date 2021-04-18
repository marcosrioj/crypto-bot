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

        Task GenerateBetterPrediction1Async(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            int limitOfDataToLearn = 1000);

        Task GenerateBetterPrediction2Async(
            List<EStrategy> strategies,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            int limitOfDataToLearn = 1000);

        RegressionDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            int limitOfDataToLearn = 120);

        Task AutoTraderBuyWithWalletVirtualAsync(long userId);

        Task AutoTraderSellWithWalletVirtualAsync();
    }
}
