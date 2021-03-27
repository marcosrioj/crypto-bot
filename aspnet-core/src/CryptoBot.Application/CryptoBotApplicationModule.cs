using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CryptoBot.Authorization;

namespace CryptoBot
{
    [DependsOn(
        typeof(CryptoBotCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class CryptoBotApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<CryptoBotAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(CryptoBotApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
