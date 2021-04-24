using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;

namespace CryptoBot.Crypto.Helpers
{
    public static class CurrencyHelper
    {
        public static IEnumerable<ECurrency> GetCurrencies(string currencies)
        {
            if (!string.IsNullOrWhiteSpace(currencies))
            {
                var coins = new List<ECurrency>();
                var currenciesSplited = currencies.Split(',');

                foreach (var activeCurrency in currenciesSplited)
                {
                    var currencyTrimmed = activeCurrency.Trim().ToUpper();
                    if (currencyTrimmed.Length >= 2)
                    {
                        var coinEnum = (ECurrency)Enum.Parse(typeof(ECurrency), currencyTrimmed);

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
                return new ECurrency[] { ECurrency.BTC, ECurrency.ETH };
            }
        }
    }
}
