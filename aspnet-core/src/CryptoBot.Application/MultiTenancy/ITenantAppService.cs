using Abp.Application.Services;
using CryptoBot.MultiTenancy.Dto;

namespace CryptoBot.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

