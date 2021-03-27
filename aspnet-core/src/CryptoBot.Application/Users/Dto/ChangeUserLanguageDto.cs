using System.ComponentModel.DataAnnotations;

namespace CryptoBot.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}