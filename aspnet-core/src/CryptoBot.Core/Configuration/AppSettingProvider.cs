using System.Collections.Generic;
using Abp.Configuration;

namespace CryptoBot.Configuration
{
    public class AppSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new[]
            {
                new SettingDefinition(AppSettingNames.UiTheme, "red", scopes: SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(CryptoBotNames.ActiveCurrencies, ""),
                new SettingDefinition(CryptoBotNames.BinanceKey, "", scopes: SettingScopes.User),
                new SettingDefinition(CryptoBotNames.BinanceSecret, "", scopes: SettingScopes.User),
            };
        }
    }
}
