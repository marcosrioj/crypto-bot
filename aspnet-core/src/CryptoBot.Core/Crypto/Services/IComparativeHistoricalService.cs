using Abp.Domain.Services;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface IComparativeHistoricalService : IDomainService
    {
        Task<Guid?> GenerateGroupComparativeHistorical(
            EApproachTrading approachTrading,
            ECurrency currency,
            KlineInterval interval,
            int limitOfDetails = 100,
            long? userId = null,
            IEnumerable<Tuple<DateTime, DateTime>> periods = null);
    }
}
