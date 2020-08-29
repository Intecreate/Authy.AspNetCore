using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Authy.AspNetCore
{
    public class AuthyUser
    {
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
    }


    public class AuthyClient : IAuthyClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AuthyClient> _logger;
        private readonly IHttpClientFactory _factory;
        private readonly HttpClient _client;
        private readonly AuthyCredentials _cred;

        public AuthyClient(AuthyCredentials cred, IHttpClientFactory clientFactory, ILogger<AuthyClient> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _factory = _clientFactory;
            _cred = cred;
            _client = _factory.CreateClient();
            _client.BaseAddress = new Uri("https://api.authy.com");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("user-agent", "AuthyAspNetCore/0.1.11");
            _client.DefaultRequestHeaders.Add("X-Authy-API-Key", _cred.ApiKey);
        }

        public async Task<string> RegisterUserAsync<T>(AuthyUser authyUser, UserManager<T> manager, T user) where T : IdentityUser
        {
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("user[email]", authyUser.Email),
                new KeyValuePair<string, string>("user[cellphone]", authyUser.PhoneNumber),
                new KeyValuePair<string, string>("user[country_code]", authyUser.CountryCode),
            });

            var result = await _client.PostAsync("/protected/json/users/new", requestContent);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            var response = await result.Content.ReadAsStreamAsync();

            using (JsonDocument document = await JsonDocument.ParseAsync(response))
            {
                if (document.RootElement.GetProperty("success").GetBoolean())
                {
                    var userId = document.RootElement.GetProperty("user").GetProperty("id").GetInt64().ToString();
                    await manager.SetAuthenticationTokenAsync(user, AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME, "UserId", userId);

                    return userId;
                }
            }

            return null;
        }

        public Task<AuthyUser> GetUserAsync<T>(UserManager<T> manager, T user) where T : IdentityUser
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveUserAsync<T>(UserManager<T> manager, T user) where T : IdentityUser
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateVerification<T>(UserManager<T> manager, T user, VerificationType verificationType, bool force = false) where T : IdentityUser
        {
            var userId = await manager.GetAuthenticationTokenAsync(user, AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME, "UserId");

            if (userId == null)
            {
                return false;
            }
            HttpResponseMessage result;

            switch (verificationType)
            {
                case VerificationType.SMS:
                case VerificationType.TOTP:
                    var shouldAppendForce = verificationType == VerificationType.SMS && force;
                    result = await _client.GetAsync($"/protected/json/sms/{userId}{ (shouldAppendForce ? "?force=true" : "")}");
                    break;
                case VerificationType.VOICE:
                    result = await _client.GetAsync($"/protected/json/call/{userId}{(force ? "?force=true" : "")}");
                    break;
                case VerificationType.PUSH:
                    throw new InvalidOperationException("To use the PUSH verificaiton, you should use CreatePushVerification<T>(UserManager<T> manager, T user)");
                default:
                    return false;
            }

            var response = await result.Content.ReadAsStreamAsync();

            using (JsonDocument document = await JsonDocument.ParseAsync(response))
            {
                return document.RootElement.GetProperty("success").GetBoolean();
            }
        }

        public async Task<string> CreateOneTouchPush<T>(UserManager<T> manager, T user, AuthyOneTouchDetails details) where T : IdentityUser
        {

            var userId = await manager.GetAuthenticationTokenAsync(user, AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME, "UserId");

            if (userId == null)
            {
                return null;
            }

            Dictionary<string, string> encContent = new Dictionary<string, string>();
            encContent.Add("message", details.Message);

            if (details.Details != null)
            {
                foreach (var d in details.Details)
                {
                    encContent.Add($"details[{d.Key}]", d.Value);
                }
            }

            if (details.HiddenDetails != null)
            {
                foreach (var d in details.HiddenDetails)
                {
                    encContent.Add($"hidden_details[{d.Key}]", d.Value);
                }
            }

            if (details.Logos != null)
            {
                foreach (var d in details.Logos)
                {
                    encContent.Add($"logos[{d.Key}]", d.Value);
                }
            }
            encContent.Add("seconds_to_expire", details.SecondsToExpire.ToString());

            var requestContent = new FormUrlEncodedContent(encContent);
            var result = await _client.PostAsync($"/onetouch/json/users/{userId}/approval_requests", requestContent);

            using (JsonDocument document = await JsonDocument.ParseAsync(await result.Content.ReadAsStreamAsync()))
            {
                return document.RootElement.GetProperty("approval_request").GetProperty("uuid").GetString();
            }
        }

        public async Task<bool> SetUserPreferredVerificationTypeAsync<T>(UserManager<T> manager, T user, VerificationType verificationType) where T : IdentityUser
        {
            await manager.SetAuthenticationTokenAsync(user, AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME, "PrefVerificaiton", verificationType.ToString());
            return true;
        }

        public async Task<VerificationType> GetUserPreferredVerificationTypeAsync<T>(UserManager<T> manager, T user) where T : IdentityUser
        {
            var token = await manager.GetAuthenticationTokenAsync(user, AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME, "PrefVerificaiton");
            if (token != null && Enum.TryParse<VerificationType>(token, out var res))
            {
                return res;
            }
            return VerificationType.UNKNOWN;
        }
    }
}
