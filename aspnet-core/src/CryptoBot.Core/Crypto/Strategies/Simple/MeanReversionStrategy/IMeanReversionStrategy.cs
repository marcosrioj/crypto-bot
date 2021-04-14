using Abp.Domain.Services;

namespace CryptoBot.Crypto.Strategies.Simple.MeanReversionStrategy
{
    public interface IMeanReversionStrategy : IDomainService, ISimpleStrategy
    {
    }
}
