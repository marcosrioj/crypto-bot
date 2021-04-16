using Abp.Domain.Entities.Auditing;
using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Entities
{
    public class Trading : AuditedEntity<long>
    {
        public decimal StartBalance { get; set; }
        public decimal EndBalance { get; set; }

        public long WalletId { get; set; }
        public Wallet Wallet { get; set; }
    }
}
