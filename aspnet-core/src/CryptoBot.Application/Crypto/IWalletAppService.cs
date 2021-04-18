using System.Threading.Tasks;
using Abp.Application.Services;
using CryptoBot.Authorization.Accounts.Dto;

namespace CryptoBot.Crypto
{
    public interface IWalletAppService : IApplicationService
    {
        Task<decimal> MyUsdtWallet(long userId);
    }
}
