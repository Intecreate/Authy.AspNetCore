namespace Authy.AspNetCore
{
    public class AuthyCredentials
    {
        public AuthyCredentials(string apiKey)
        {
            ApiKey = apiKey;
        }

        internal string ApiKey { get; set; }
    }
}
