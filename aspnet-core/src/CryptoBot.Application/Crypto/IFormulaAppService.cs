using Abp.Application.Services;
using CryptoBot.Crypto.Services.Dtos;
using System.Threading.Tasks;

namespace CryptoBot.Crypto
{
    public interface IFormulaAppService : IAsyncCrudAppService<FormulaDto, long>
    {
        Task<bool> Disable(long? formulaId);
        Task<bool> Enable(long formulaId);
    }
}
