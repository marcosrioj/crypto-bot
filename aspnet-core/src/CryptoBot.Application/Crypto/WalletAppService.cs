using CryptoBot.Crypto.Services;
using System.Threading.Tasks;

namespace CryptoBot.Crypto
{
    public class WalletAppService : CryptoBotAppServiceBase, IWalletAppService
    {
        private readonly IWalletService _walletService;

        public WalletAppService(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<decimal> MyUsdtWallet(long userId)
        {
            return await _walletService.MyUsdtWallet(userId);
        }
    }
}
