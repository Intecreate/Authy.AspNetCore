﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Authy.AspNetCore
{
    public class AuthyTwoFactorTokenProvider<T> : IUserTwoFactorTokenProvider<T> where T : IdentityUser
    {
        private readonly ILogger<AuthyTwoFactorTokenProvider<T>> _logger;
        private readonly IHttpClientFactory _factory;
        private readonly AuthyCredentials _cred;

        public AuthyTwoFactorTokenProvider(AuthyCredentials cred, ILogger<AuthyTwoFactorTokenProvider<T>> logger, IHttpClientFactory factory)
        {
            _logger = logger;
            _factory = factory;
            _cred = cred;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<T> manager, T user)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //var appUser = user as IdentityUser;
            return false;
        }

        public Task<string> GenerateAsync(string purpose, UserManager<T> manager, T user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<T> manager, T user)
        {
            var claim = (await manager.GetClaimsAsync(user)).FirstOrDefault(c => c.Issuer == "Authy.AspNetCore" && c.Type == "authy.userid");

            if (claim == null)
            {
                return false;
            }

            var client = _factory.CreateClient();
            client.BaseAddress = new Uri("https://api.authy.com");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("user-agent", "IntecreateAuthy");
            client.DefaultRequestHeaders.Add("X-Authy-API-Key", _cred.ApiKey);

            var result = await client.GetAsync($"/protected/json/verify/{token}/{claim.Value}");

            _logger.LogDebug(result.ToString());
            _logger.LogDebug(result.Content.ReadAsStringAsync().Result);

            var message = await result.Content.ReadAsStringAsync();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }
    }
}
