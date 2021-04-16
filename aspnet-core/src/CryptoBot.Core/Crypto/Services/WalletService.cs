using Abp.Domain.Repositories;
using Abp.Domain.Services;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class WalletService : DomainService, IWalletService
    {
        public readonly IRepository<Wallet, long> _repository;

        public WalletService(IRepository<Wallet, long> repository)
        {
            _repository = repository;
        }

        public async Task<Wallet> GetOrCreate(ECurrency currency, EWalletType type, long userId, decimal initialBalance = 0)
        {
            var wallet = await _repository
                .GetAll()
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Currency == currency)
                .FirstOrDefaultAsync();

            if (wallet == null)
            {
                var walletId = await _repository.InsertAndGetIdAsync(new Wallet
                {
                    Currency = currency,
                    Type = type,
                    Balance = initialBalance,
                    UserId = userId
                });

                wallet = await _repository
                    .GetAll()
                    .AsNoTracking()
                    .Where(x => x.Id == walletId)
                    .FirstOrDefaultAsync();
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return wallet;
        }
    }
}
