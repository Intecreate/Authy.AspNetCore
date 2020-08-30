using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authy.AspNetCore.Tests.TokenProvider
{
    public class TestAuthyTwoFactorTokenProvider : TestAuthyTwoFactorTokenProviderHelpers
    {
        public TestAuthyTwoFactorTokenProvider() : base() { }

        [Fact]
        public async Task TestCanGenerateTwoFactorTokenAsync()
        {
            var provider = CreateProvider(null);
            Assert.True(await provider.CanGenerateTwoFactorTokenAsync(userManager.Object, testUser));
            Assert.False(await provider.CanGenerateTwoFactorTokenAsync(userManager.Object, testUser2));
        }

        [Fact]
        public async Task TestGenerateAsync()
        {
            var provider = CreateProvider(null);
            Assert.Equal(string.Empty, await provider.GenerateAsync("none", userManager.Object, testUser));
            Assert.Equal(string.Empty, await provider.GenerateAsync(null, null, null));
        }

        [Fact]
        public async Task TestValidateAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\": \"Token is valid.\",\"token\": \"is valid\",\"success\": \"true\"}")
            };

            var provider = CreateProvider(res);
            Assert.True(await provider.ValidateAsync(null, "123456", userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidateOkFailAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\": \"Token is not valid.\",\"token\": \"is not valid\",\"success\": \"false\"}")
            };

            var provider = CreateProvider(res);
            Assert.False(await provider.ValidateAsync(null, "123456", userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidatePushOkAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true, \"approval_request\": {\"status\": \"approved\"}}")
            };

            var provider = CreateProvider(res);
            Assert.True(await provider.ValidateAsync(null, Guid.NewGuid().ToString(), userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidatePushOkFailAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\": false, \"approval_request\": {\"status\": \"declined\"}}")
            };

            var provider = CreateProvider(res);
            Assert.False(await provider.ValidateAsync(null, Guid.NewGuid().ToString(), userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidateFailUnauthAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"message\": \"Token is not valid.\",\"token\": \"is not valid\",\"success\": \"false\"}")
            };

            var provider = CreateProvider(res);
            Assert.False(await provider.ValidateAsync(null, "123456", userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidatePushFailUnauthAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"success\": false, \"approval_request\": {\"status\": \"declined\"}}")
            };

            var provider = CreateProvider(res);
            Assert.False(await provider.ValidateAsync(null, Guid.NewGuid().ToString(), userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidatePushFailOkSuccessDeclineAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true, \"approval_request\": {\"status\": \"declined\"}}")
            };

            var provider = CreateProvider(res);
            Assert.False(await provider.ValidateAsync(null, Guid.NewGuid().ToString(), userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidatePushOkayMalformedContentAsync()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{}")
            };

            var provider = CreateProvider(res);
            Assert.False(await provider.ValidateAsync(null, Guid.NewGuid().ToString(), userManager.Object, testUser));
        }

        [Fact]
        public async Task TestValidateNoUser()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\": false, \"approval_request\": {\"status\": \"declined\"}}")
            };

            var provider = CreateProvider(res);
            Assert.False(await provider.ValidateAsync(null, "123456", userManager.Object, testUser2));
        }
    }
}
