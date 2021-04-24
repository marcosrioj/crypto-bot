using Abp.Domain.Entities.Auditing;

namespace CryptoBot.Crypto.Entities
{
    public class Robot : FullAuditedEntity<long>
    {
        public bool IsActive { get; set; }

        public long FormulaId { get; set; }
        public Formula Formula { get; set; }

        public long UserId { get; set; }
        public Authorization.Users.User User { get; set; }
    }
}
