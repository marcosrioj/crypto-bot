using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.Configuration;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;

namespace CryptoBot.EntityFrameworkCore.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly CryptoBotDbContext _context;

        public DefaultSettingsCreator(CryptoBotDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            int? tenantId = null;

            if (CryptoBotConsts.MultiTenancyEnabled == false)
            {
                tenantId = MultiTenancyConsts.DefaultTenantId;
            }

            // Emailing
            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "admin@mydomain.com", tenantId);
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "mydomain.com mailer", tenantId);

            // Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en", tenantId);

            //Binance General
            AddSettingIfNotExists(CryptoBotNames.ActiveCurrencies, "BTC,ETH,BNB,ONE,ANKR");

            //Binance Marcos
            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 2);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 2);
        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null, long? userId = null)
        {
            if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == userId))
            {
                return;
            }

            _context.Settings.Add(new Setting(tenantId, userId, name, value));
            _context.SaveChanges();
        }
    }
}
