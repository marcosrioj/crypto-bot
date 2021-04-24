using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Crypto.Entities
{
    public class Robot : FullAuditedEntity<long>
    {
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18, 8)")]
        public decimal InitialAmount { get; set; }

        public long FormulaId { get; set; }
        public Formula Formula { get; set; }

        public long UserId { get; set; }
        public Authorization.Users.User User { get; set; }
    }
}
