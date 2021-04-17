using Abp.Domain.Services;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface IWalletService : IDomainService
    {
        Task<Wallet> GetOrCreate(ECurrency currency, EWalletType type, long userId, decimal initialBalance = 0);

        Task UpdateBalance(long walletId, decimal balance);
    }
}
