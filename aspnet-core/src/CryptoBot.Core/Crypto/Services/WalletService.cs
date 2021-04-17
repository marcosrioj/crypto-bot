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

        public async Task UpdateBalance(long walletId, decimal balance)
        {
            var wallet = await _repository
                .GetAll()
                .Where(x => x.Id == walletId)
                .FirstAsync();

            wallet.Balance = balance;

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task UpdatedWalletsUsdtToCustomCurrency(long userId, decimal deductionUsdt, ECurrency currency, decimal amount)
        {
            var mainWallet = await GetOrCreate(ECurrency.USDT, EWalletType.Virtual, userId);
            var newMainWalletBalance = mainWallet.Balance - deductionUsdt;
            await UpdateBalance(mainWallet.Id, newMainWalletBalance);

            var wallet = await GetOrCreate(currency, EWalletType.Virtual, userId);
            var newWalletBalance = wallet.Balance + amount;
            await UpdateBalance(wallet.Id, newWalletBalance);
        }

        public async Task UpdatedWalletsCustomCurrencyToUsdt(long userId, decimal deductionCurrency, ECurrency currency, decimal amount)
        {
            var wallet = await GetOrCreate(currency, EWalletType.Virtual, userId);
            var newWalletBalance = wallet.Balance - deductionCurrency;
            await UpdateBalance(wallet.Id, newWalletBalance);

            var mainWallet = await GetOrCreate(ECurrency.USDT, EWalletType.Virtual, userId);
            var newMainWalletBalance = mainWallet.Balance + amount;
            await UpdateBalance(mainWallet.Id, newMainWalletBalance);
        }
    }
}
