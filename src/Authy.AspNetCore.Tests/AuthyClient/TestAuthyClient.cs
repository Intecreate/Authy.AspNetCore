using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authy.AspNetCore.Tests
{
    public class TestAuthyClient : TestAuthyClientHelpers
    {
        public TestAuthyClient() : base() { }

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

        [Fact]
        public async Task TestGetUserAsync()
        {
            //Not yet implemented
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            await Assert.ThrowsAsync<NotImplementedException>(() => CreateClient(res).GetUserAsync(userManager.Object, testUser));
        }

        [Fact]
        public async Task TestRemoveUserAsync()
        {
            //Not yet implemented
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            await Assert.ThrowsAsync<NotImplementedException>(() => CreateClient(res).RemoveUserAsync(userManager.Object, testUser));
        }

        [Fact]
        public async Task TestGetSetUserPreferredVerificationTypeAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var authyClient = CreateClient(res);
            Assert.Equal(VerificationType.UNKNOWN, await authyClient.GetUserPreferredVerificationTypeAsync(userManager.Object, testUser));

            await authyClient.SetUserPreferredVerificationTypeAsync(userManager.Object, testUser, VerificationType.PUSH);
            Assert.Equal(VerificationType.PUSH, await authyClient.GetUserPreferredVerificationTypeAsync(userManager.Object, testUser));

            await authyClient.SetUserPreferredVerificationTypeAsync(userManager.Object, testUser, VerificationType.SMS);
            Assert.Equal(VerificationType.SMS, await authyClient.GetUserPreferredVerificationTypeAsync(userManager.Object, testUser));

            await authyClient.SetUserPreferredVerificationTypeAsync(userManager.Object, testUser, VerificationType.VOICE);
            Assert.Equal(VerificationType.VOICE, await authyClient.GetUserPreferredVerificationTypeAsync(userManager.Object, testUser));

            await authyClient.SetUserPreferredVerificationTypeAsync(userManager.Object, testUser, VerificationType.TOTP);
            Assert.Equal(VerificationType.TOTP, await authyClient.GetUserPreferredVerificationTypeAsync(userManager.Object, testUser));
        }
    }
}
