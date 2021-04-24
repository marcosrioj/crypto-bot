using Abp.Application.Services;
using CryptoBot.Crypto.Services.Dtos;

namespace CryptoBot.Crypto
{
    public interface IFormulaAppService : IAsyncCrudAppService<FormulaDto, long>
    {
    }
}
