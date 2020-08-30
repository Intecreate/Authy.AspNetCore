using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Authy.AspNetCore.Tests.TokenProvider
{
    public class TestAuthyTwoFactorTokenProviderHelpers
    {
        public Mock<IHttpClientFactory> clientFactory;
        public Mock<IUserStore<IdentityUser>> store;
        public Mock<UserManager<IdentityUser>> userManager;
        public IdentityUser testUser;
        public IdentityUser testUser2;

        public TestAuthyTwoFactorTokenProviderHelpers()
        {
            store = new Mock<IUserStore<IdentityUser>>();
            testUser = new IdentityUser()
            {
                Id = "123"
            };
            testUser2 = new IdentityUser()
            {
                Id = "abc"
            };
            store.Setup(x => x.FindByIdAsync("123", CancellationToken.None)).ReturnsAsync(testUser);
            store.Setup(x => x.FindByIdAsync("abc", CancellationToken.None)).ReturnsAsync(testUser2);

            userManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(m => m.SetAuthenticationTokenAsync(testUser, "Authy", "UserId", "123")).ReturnsAsync(new IdentityResult());
            userManager.Setup(m => m.GetAuthenticationTokenAsync(testUser, "Authy", "UserId")).ReturnsAsync("123");
        }

        public AuthyTwoFactorTokenProvider<IdentityUser> CreateProvider(HttpResponseMessage res)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(res);

            clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHttpMessageHandler.Object));

            return new AuthyTwoFactorTokenProvider<IdentityUser>(new AuthyCredentials(""), null, clientFactory.Object);
        }
    }
}
