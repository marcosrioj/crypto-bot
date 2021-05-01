using Abp.Domain.Services;
using CryptoBot.Crypto.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface ISettingsService : IDomainService
    {
        float GetInvestorProfileFactor(EStrategy strategy, EProfitWay profitWay, EInvestorProfile investorProfile = EInvestorProfile.UltraConservative);
    }
}
