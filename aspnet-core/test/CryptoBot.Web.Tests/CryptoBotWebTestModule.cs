using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CryptoBot.EntityFrameworkCore;
using CryptoBot.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace CryptoBot.Web.Tests
{
    [DependsOn(
        typeof(CryptoBotWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class CryptoBotWebTestModule : AbpModule
    {
        public CryptoBotWebTestModule(CryptoBotEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CryptoBotWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(CryptoBotWebMvcModule).Assembly);
        }
    }
}