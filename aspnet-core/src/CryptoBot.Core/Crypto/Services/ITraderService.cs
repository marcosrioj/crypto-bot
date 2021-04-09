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
        Task<WhatToDoOutput> WhatToDo(RegressionDataOutput data);

        Task<WhatToDoOutput> WhatToDo(
            EStrategy strategy,
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
                    RegressionDataOutput data,
                    ELogLevel logLevel = ELogLevel.NoLog);

        Task<List<BetterCoinsToTraderRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<List<BetterCoinsToTraderRightNowOutputDto>> FilterBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            List<BetterCoinsToTraderRightNowOutputDto> input);
    }
}
