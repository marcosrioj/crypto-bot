using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using CryptoBot.Configuration.Dto;

namespace CryptoBot.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : CryptoBotAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
