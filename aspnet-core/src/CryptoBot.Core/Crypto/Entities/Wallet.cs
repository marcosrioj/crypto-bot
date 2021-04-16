using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Entities
{
    public class Wallet : CreationAuditedEntity<long>
    {
        public EWalletType Type { get; set; }
        public ECurrency Currency { get; set; }
        public decimal Balance { get; set; }

        public long UserId { get; set; }
        public Authorization.Users.User User { get; set; }
    }
}
