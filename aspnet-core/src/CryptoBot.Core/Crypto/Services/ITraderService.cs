using System;
using Abp.Domain.Services;
using Binance.Net.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Net.Enums;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderService : IDomainService
    {
        Task<EWhatToDo> WhatToDo(
            ECurrency currency,
            KlineInterval interval,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int limitOfDetails = 1000);

        Task<EWhatToDo> WhatToDo(
            EStrategy strategy,
            ECurrency currency,
            KlineInterval interval,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int limitOfDetails = 1000);
    }
}
