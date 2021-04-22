using Abp.Dependency;
using Abp.Quartz;
using CryptoBot.Crypto.Services;
using Quartz;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Background.Jobs
{
    [DisallowConcurrentExecution]
    public class SellVirtualTraderJob : JobBase, ITransientDependency
    {
        private readonly ITraderService _traderService;

        public SellVirtualTraderJob(ITraderService traderService)
        {
            _traderService = traderService;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await _traderService.AutoTraderSellWithWalletVirtualAsync();
        }
    }
}
