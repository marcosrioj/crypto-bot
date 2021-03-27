using Abp.Domain.Services;
using CryptoBot.Crypto.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ICurrencyService : IDomainService
    {
        Task<IEnumerable<ECurrency>> GetActiveCurrencies();
    }
}
