using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services.Dtos;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderTestService : IDomainService
    {
        Task<IEnumerable<CompleteRegressionTestOutputDto>> CompleteRegressionTest(
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDetailsToLearnAndTest = 1000,
            int limitOfDetailsToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<List<RegressionTestOutputDto>> RegressionTest(
            EStrategy strategy,
            RegressionTestDataOutput data,
            ELogLevel logLevel = ELogLevel.NoLog);

        RegressionTestDataOutput GetRegressionDataTest(
            ECurrency currency,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 240,
            int limitOfDataToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<IEnumerable<BetterCoinsToTraderRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null);
    }
}
