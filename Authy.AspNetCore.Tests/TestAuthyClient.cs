using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Authy.AspNetCore.Tests
{
    public class TestAuthyClient
    {
        Mock<IHttpClientFactory> clientFactory;
        Mock<IUserStore<IdentityUser>> store;
        Mock<UserManager<IdentityUser>> userManager;
        IdentityUser testUser;

        public TestAuthyClient()
        {
            store = new Mock<IUserStore<IdentityUser>>();
            testUser = new IdentityUser()
            {
                Id = "123"
            };
            store.Setup(x => x.FindByIdAsync("123", CancellationToken.None)).ReturnsAsync(testUser);

            userManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(m => m.SetAuthenticationTokenAsync(It.IsAny<IdentityUser>(), "Authy", "UserId", "123")).ReturnsAsync(new IdentityResult());
        }

        private IAuthyClient CreateClient(HttpResponseMessage res)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(res);

            clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHttpMessageHandler.Object));

            return new AuthyClient(new AuthyCredentials(""), clientFactory.Object, null);
        }

        [Fact]
        public async Task TestRegisterUserAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\": \"User created successfully.\",\"user\": {\"id\": 123456 },\"success\": true}")
            };

            var authyClient = CreateClient(res);
            var userId = await authyClient.RegisterUserAsync(new AuthyUser(), userManager.Object, await userManager.Object.FindByIdAsync("123"));
            Assert.Equal("123456", userId);
        }

        [Fact]
        public async Task TestRegisterUserFailAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\": \"Failed to create user.\",\"success\": false}")
            };

            var authyClient = CreateClient(res);
            var userId = await authyClient.RegisterUserAsync(new AuthyUser(), userManager.Object, testUser);
            Assert.Null(userId);
        }

        [Fact]
        public async Task TestRegisterUserNotOkayStatusCodeAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var authyClient = CreateClient(res);
            var userId = await authyClient.RegisterUserAsync(new AuthyUser(), userManager.Object, testUser);
            Assert.Null(userId);
        }
    }
}
