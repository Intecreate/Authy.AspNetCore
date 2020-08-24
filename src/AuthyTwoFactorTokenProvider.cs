using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Authy.AspNetCore
{
    public class AuthyTwoFactorTokenProvider<T> : IUserTwoFactorTokenProvider<T> where T : IdentityUser
    {
        private readonly ILogger<AuthyTwoFactorTokenProvider<T>> _logger;
        private readonly IHttpClientFactory _factory;
        private readonly AuthyCredentials _cred;
        private readonly HttpClient _client;

        public AuthyTwoFactorTokenProvider(AuthyCredentials cred, ILogger<AuthyTwoFactorTokenProvider<T>> logger, IHttpClientFactory factory)
        {
            _logger = logger;
            _factory = factory;
            _cred = cred;
            _client = factory.CreateClient();

            _client.BaseAddress = new Uri("https://api.authy.com");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("user-agent", "AuthyAspNetCore/0.1.9");
            _client.DefaultRequestHeaders.Add("X-Authy-API-Key", _cred.ApiKey);
        }

        public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<T> manager, T user)
        {
            var key = await manager.GetAuthenticationTokenAsync(user, "Authy", "UserId");
            return !string.IsNullOrWhiteSpace(key);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<string> GenerateAsync(string purpose, UserManager<T> manager, T user)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return string.Empty;
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<T> manager, T user)
        {
            HttpResponseMessage result;
            if (token.Length <= 10)
            {
                var userId = await manager.GetAuthenticationTokenAsync(user, "Authy", "UserId");

                if (userId == null)
                {
                    return false;
                }

                result = await _client.GetAsync($"/protected/json/verify/{token}/{userId}");

                var message = await result.Content.ReadAsStringAsync();
                _logger.LogDebug(message);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            else
            {
                result = await _client.GetAsync($"/onetouch/json/approval_requests/{token}");

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                var response = await result.Content.ReadAsStreamAsync();

                using (JsonDocument document = await JsonDocument.ParseAsync(response))
                {
                    if (document.RootElement.GetProperty("success").GetBoolean())
                    {
                        var status = document.RootElement.GetProperty("approval_request").GetProperty("status").GetString();

                        return status == "approved";
                    }
                }
            }

            return false;
        }
    }
}
