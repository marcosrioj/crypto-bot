using Abp.Application.Services;
using CryptoBot.Crypto.Dtos.Services;

namespace CryptoBot.Crypto
{
    public interface IFormulaAppService : IAsyncCrudAppService<FormulaDto, long>
    {
    }
}
