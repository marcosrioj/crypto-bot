using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ITraderService : IDomainService
    {
        Task<EWhatToDo> WhatToDo(RegressionTestDataOutput data);

        Task<EWhatToDo> WhatToDo(
            EStrategy strategy,
            RegressionTestDataOutput data);
    }
}
