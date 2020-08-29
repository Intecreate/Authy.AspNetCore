namespace Authy.AspNetCore
{
    /// <summary>
    /// A class to contain the Authy apiKey
    /// </summary>
    public class AuthyCredentials
    {
        /// <summary>
        /// A class to contain the Authy apiKey
        /// </summary>
        /// <param name="apiKey">Your authy api key</param>
        public AuthyCredentials(string apiKey)
        {
            ApiKey = apiKey;
        }

        /// <summary>
        /// The Api key, for internal use only
        /// </summary>
        internal string ApiKey { get; set; }
    }
}
