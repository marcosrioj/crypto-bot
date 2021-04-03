using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderService : IDomainService
    {
        Task<EWhatToDo> WhatToDo(
            ECurrency currency);

        Task<EWhatToDo> WhatToDo(
            EStrategy strategy,
            ECurrency currency,
            IBinanceKline? sampleStock = null);

        void SetData(ECurrency currency, KlineInterval interval, DateTime? startTime, DateTime? endTime,
            int limitOfDetails);

        void SetData(ECurrency currency, List<IBinanceKline> inputData);
    }
}
