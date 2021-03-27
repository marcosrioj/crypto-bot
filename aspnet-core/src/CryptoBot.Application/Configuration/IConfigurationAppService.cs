using System.Threading.Tasks;
using CryptoBot.Configuration.Dto;

namespace CryptoBot.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
