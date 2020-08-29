using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Authy.AspNetCore
{

    public static class AuthyBuilder
    {
        public const string AUTHY_TOKEN_PROVIDER_NAME = "Authy";

        public static IdentityBuilder AddAuthy2FA<T>(this IdentityBuilder builder) where T : IdentityUser
        {
            return builder.AddTokenProvider(AUTHY_TOKEN_PROVIDER_NAME, typeof(AuthyTwoFactorTokenProvider<T>));
        }

        public static IServiceCollection AddAuthy(this IServiceCollection services, AuthyCredentials credentials)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton(credentials);
            services.AddTransient<IAuthyClient, AuthyClient>();

            return services;
        }
    }
}
