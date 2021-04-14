using Abp.Domain.Services;
using CryptoBot.Crypto.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ISettingsService : IDomainService
    {
        Task<IEnumerable<ECurrency>> GetActiveCurrencies();
        Task<float> GetInvestorProfileFactor(EStrategy strategy, EInvestorProfile investorProfile = EInvestorProfile.UltraAggressive);
    }
}
