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

        Task<RegressionTestOutputDto> RegressionTest(
            EStrategy strategy,
            ECurrency currency,
            KlineInterval interval,
            decimal initialWallet,
            ELogLevel logLevel = ELogLevel.NoLog,
            int limitOfDetailsToLearnAndTest = 1000,
            int limitOfDetailsToTest = 120,
            DateTime? startTime = null,
            DateTime? endTime = null);
    }
}
