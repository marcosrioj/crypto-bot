using Abp.Application.Services;
using CryptoBot.Crypto.Services.Dtos;
using System.Threading.Tasks;

namespace CryptoBot.Crypto
{
    public interface IRobotAppService : IAsyncCrudAppService<RobotDto, long>
    {
        Task<bool> Disable(long? robotId);
        Task<bool> Enable(long robotId);
    }
}
