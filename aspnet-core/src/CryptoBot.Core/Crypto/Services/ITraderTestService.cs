using System;
using System.Threading.Tasks;
using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderTestService : IDomainService
    {
        Task RegressionTest(
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
