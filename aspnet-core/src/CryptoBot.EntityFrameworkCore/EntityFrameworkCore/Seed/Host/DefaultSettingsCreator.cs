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

            //Binance Marcos
            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 2);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 2);

            //Binance Tests
            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 3);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 3);

            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 4);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 4);

            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 5);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 5);

            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 6);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 6);

            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 7);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 7);

            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 8);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 8);

            AddSettingIfNotExists(CryptoBotNames.BinanceKey, "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy", null, 9);
            AddSettingIfNotExists(CryptoBotNames.BinanceSecret, "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g", null, 9);

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
