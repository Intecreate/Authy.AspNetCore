using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Authy.AspNetCore
{
    public class AuthyUser
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
        [JsonPropertyName("cellphone")]
        public string PhoneNumber { get; set; }
    }

    class LazyAuthyHelper
    {
        [JsonPropertyName("user")]
        public AuthyUser User { get; set; }
    }


    public class AuthyCall2FA : IAuthyCall2FA
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AuthyCall2FA> _logger;
        private readonly IHttpClientFactory _factory;
        private readonly AuthyCredentials _cred;

        public AuthyCall2FA(AuthyCredentials cred, IHttpClientFactory clientFactory, ILogger<AuthyCall2FA> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _factory = _clientFactory;
            _cred = cred;
        }

        public async Task<string> RegisterUserAsync<T>(AuthyUser authyUser, UserManager<T> manager, T user) where T : IdentityUser
        {
            var postDataJson = JsonSerializer.Serialize(new LazyAuthyHelper { User = authyUser });
            var content = new StringContent(postDataJson, Encoding.UTF8, "application/json");

            var client = _factory.CreateClient();

            client.BaseAddress = new Uri("https://api.authy.com");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("user-agent", "IntecreateAuthy");
            client.DefaultRequestHeaders.Add("X-Authy-API-Key", _cred.ApiKey);
            var result = await client.PostAsync("/protected/json/users/new", content);

            _logger.LogDebug(result.Content.ReadAsStringAsync().Result);

            result.EnsureSuccessStatusCode();

            var response = await result.Content.ReadAsStringAsync();


            using (JsonDocument document = JsonDocument.Parse(response))
            {
                if (document.RootElement.GetProperty("success").GetBoolean())
                {
                    var userId = document.RootElement.GetProperty("user").GetProperty("id").GetInt64().ToString();
                    var claim = new Claim("authy.userid", userId, "uid", "Authy.AspNetCore");
                    await manager.AddClaimAsync(user, claim);

                    return userId;
                }
            }

            return null;
        }
    }
}
