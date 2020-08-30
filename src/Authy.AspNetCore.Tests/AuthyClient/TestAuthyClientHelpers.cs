using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Authy.AspNetCore.Tests
{
    public class TestAuthyClientHelpers
    {
        public Mock<IHttpClientFactory> clientFactory;
        public Mock<IUserStore<IdentityUser>> store;
        public Mock<UserManager<IdentityUser>> userManager;
        public IdentityUser testUser;
        public string prefVerification;

        public TestAuthyClientHelpers()
        {
            store = new Mock<IUserStore<IdentityUser>>();
            testUser = new IdentityUser()
            {
                Id = "123"
            };
            store.Setup(x => x.FindByIdAsync("123", CancellationToken.None)).ReturnsAsync(testUser);

            userManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(m => m.SetAuthenticationTokenAsync(testUser, "Authy", "UserId", "123")).ReturnsAsync(new IdentityResult());
            userManager.Setup(m => m.GetAuthenticationTokenAsync(testUser, "Authy", "UserId")).ReturnsAsync("123");
            userManager.Setup(m => m.SetAuthenticationTokenAsync(testUser, "Authy", "PrefVerificaiton", It.IsAny<string>())).Callback<IdentityUser, string, string, string>((u, a, p, s) =>
            {
                userManager.Setup(m => m.GetAuthenticationTokenAsync(testUser, "Authy", "PrefVerificaiton")).ReturnsAsync(s);
            }).ReturnsAsync(new IdentityResult());

        }

        public IAuthyClient CreateClient(HttpResponseMessage res)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(res);

            clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHttpMessageHandler.Object));

            return new AuthyClient(new AuthyCredentials(""), clientFactory.Object, null);
        }
    }
}
