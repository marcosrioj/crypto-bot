using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderTestService : IDomainService
    {
        Task<IEnumerable<CompleteRegressionTestOutputDto>> CompleteRegressionTest(
            EInvestorProfile investorProfile,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDetailsToLearnAndTest = 1000,
            int limitOfDetailsToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null);

        RegressionTestDataOutput GetRegressionData(
            ECurrency currency,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 240,
            int limitOfDataToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<List<RegressionTestOutputDto>> RegressionExec(
                    EStrategy strategy,
                    EInvestorProfile eInvestorProfile,
                    RegressionTestDataOutput data,
                    ELogLevel logLevel = ELogLevel.NoLog);

        Task<List<BetterCoinsToTestTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            KlineInterval interval,
            decimal initialWallet,
            int limitOfDataToLearnAndTest = 1000,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<List<BetterCoinsToTestTradeRightNowOutputDto>> FilterBetterCoinsToTraderRightNowAsync(
            EStrategy strategy,
            EInvestorProfile eInvestorProfile,
            List<BetterCoinsToTestTradeRightNowOutputDto> input);

        Task<List<BetterCoinsToTestTradeRightNowOutputDto>> GetBetterCoinsToTraderRightNowAsync(
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
