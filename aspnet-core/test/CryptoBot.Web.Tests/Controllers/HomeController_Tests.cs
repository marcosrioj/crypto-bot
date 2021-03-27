using System.Threading.Tasks;
using CryptoBot.Models.TokenAuth;
using CryptoBot.Web.Controllers;
using Shouldly;
using Xunit;

namespace CryptoBot.Web.Tests.Controllers
{
    public class HomeController_Tests: CryptoBotWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}