using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authy.AspNetCore
{
    public static class AuthyBuilder
    {
        public static IdentityBuilder AddAuthy2FA<T>(this IdentityBuilder builder) where T : IdentityUser
        {
            var userType = builder.UserType;
            return builder.AddTokenProvider("Authy", typeof(AuthyTwoFactorTokenProvider<T>));
        }


    }
}
