using System.Threading.Tasks;
using Abp.Application.Services;
using CryptoBot.Sessions.Dto;

namespace CryptoBot.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
