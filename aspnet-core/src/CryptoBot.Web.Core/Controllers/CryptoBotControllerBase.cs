using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace CryptoBot.Controllers
{
    public abstract class CryptoBotControllerBase: AbpController
    {
        protected CryptoBotControllerBase()
        {
            LocalizationSourceName = CryptoBotConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
