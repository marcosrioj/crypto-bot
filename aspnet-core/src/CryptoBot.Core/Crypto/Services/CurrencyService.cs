﻿using Abp.Configuration;
using Abp.Domain.Services;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class CurrencyService : DomainService, ICurrencyService
    {
        private readonly ISettingManager _settingManager;

        public CurrencyService(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        public async Task<IEnumerable<ECurrency>> GetActiveCurrencies()
        {
            var activeCurrencies = await _settingManager.GetSettingValueAsync(CryptoBotNames.ActiveCurrencies);

            if (!string.IsNullOrWhiteSpace(activeCurrencies))
            {
                var coins = new List<ECurrency>();
                var activeCurrenciesSplited = activeCurrencies.Split(',');

                foreach (var activeCurrency in activeCurrenciesSplited)
                {
                    var activeCurrencyTrimmed = activeCurrency.Trim().ToLower();
                    if (activeCurrencyTrimmed.Length > 2)
                    {
                        var coinEnumValue = string.Concat(char.ToUpper(activeCurrencyTrimmed[0]), activeCurrencyTrimmed.Substring(1));
                        var coinEnum = (ECurrency)Enum.Parse(typeof(ECurrency), coinEnumValue);

                        if (Enum.IsDefined(typeof(ECurrency), coinEnum))
                        {
                            coins.Add(coinEnum);
                        }
                    }
                }

                return coins;
            }
            else
            {
                return new ECurrency[] { ECurrency.Btc, ECurrency.Eth };
            }
        }
    }
}
