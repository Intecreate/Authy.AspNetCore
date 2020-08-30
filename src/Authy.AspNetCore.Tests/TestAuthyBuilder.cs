using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Authy.AspNetCore.Tests
{
    public class TestAuthyBuilder
    {
        [Fact]
        public void EnsureProviderNameIsAuthy()
        {
            Assert.Equal("Authy", AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME);
        }

        [Fact]
        public void TestAddAuthy2FA()
        {
            var mock = new Mock<IdentityBuilder>(typeof(IdentityUser), null);
            string providerName = null;
            Type providerType = null;
            mock.Setup(b => b.AddTokenProvider(It.IsAny<string>(), It.IsAny<Type>())).Callback<string, Type>((s, t) => {
                providerName = s;
                providerType = t;
            }) ;

            mock.Object.AddAuthy2FA<IdentityUser>();

            mock.Verify(m => m.AddTokenProvider(AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME, typeof(AuthyTwoFactorTokenProvider<IdentityUser>)), Times.Once());

            Assert.Equal(AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME, providerName);
            Assert.Equal(typeof(AuthyTwoFactorTokenProvider<IdentityUser>), providerType);
        }

        [Fact]
        public void TestAddAuthyNullService()
        {
            IServiceCollection service = null;
            Assert.Throws<ArgumentNullException>(() => service.AddAuthy(new AuthyCredentials("")));
        }

        [Fact]
        public void TestAddAuthy()
        {
            var services = new ServiceCollection();
            var cred = new AuthyCredentials("key");

            services.AddAuthy(cred);

            Assert.Contains(services, s => s.ImplementationInstance == cred);
            Assert.Contains(services, s => s.ServiceType == typeof(IAuthyClient) && s.ImplementationType == typeof(AuthyClient));
        }
    }
}
