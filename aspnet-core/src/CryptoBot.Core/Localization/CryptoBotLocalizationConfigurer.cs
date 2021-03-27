using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace CryptoBot.Localization
{
    public static class CryptoBotLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(CryptoBotConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(CryptoBotLocalizationConfigurer).GetAssembly(),
                        "CryptoBot.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
