using Abp.Authorization;
using CryptoBot.Authorization.Roles;
using CryptoBot.Authorization.Users;

namespace CryptoBot.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
