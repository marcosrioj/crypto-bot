using Abp.Domain.Services;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public interface IQuoteService : IDomainService
    {
        Task AddCurrentQuoteHistoryAsync();
    }
}
