using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Authy.AspNetCore.Tests
{
    public class TestAuthyClientVerifications : TestAuthyClientHelpers
    {
        public TestAuthyClientVerifications() : base() { }

        [Fact]
        public async Task TestCreateVerificationNoUser()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true,\"message\":\"SMS token was sent\",\"cellphone\":\"+1-XXX-XXX-XX02\"}")
            };

            var authyClient = CreateClient(res);
            Assert.False(await authyClient.CreateVerification(userManager.Object, new IdentityUser(), VerificationType.SMS, false));
        }

        #region SMS
        [Fact]
        public async Task TestCreateVerificationSMSNoForceNoAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true,\"message\":\"SMS token was sent\",\"cellphone\":\"+1-XXX-XXX-XX02\"}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.SMS, false));
        }

        [Fact]
        public async Task TestCreateVerificationSMSForceNoAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true,\"message\":\"SMS token was sent\",\"cellphone\":\"+1-XXX-XXX-XX02\"}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.SMS, false));
        }

        [Fact]
        public async Task TestCreateVerificationSMSNoForceAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\":\"Ignored: SMS is not needed for smartphones. Pass force=true if you want to actually send it anyway.\",\"cellphone\":\"+1-XXX-XXX-XX02\",\"device\":\"android\",\"ignored\":true,\"success\":true}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.SMS, false));
        }

        [Fact]
        public async Task TestCreateVerificationSMSForceAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true,\"message\":\"SMS token was sent\",\"cellphone\":\"+1-XXX-XXX-XX02\"}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.SMS, true));
        }
        #endregion SMS

        #region TOTP
        [Fact]
        public async Task TestCreateVerificationTOTPNoForce()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\":\"Ignored: SMS is not needed for smartphones. Pass force=true if you want to actually send it anyway.\",\"cellphone\":\"+1-XXX-XXX-XX02\",\"device\":\"android\",\"ignored\":true,\"success\":true}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.TOTP, false));
        }

        [Fact]
        public async Task TestCreateVerificationForce()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\":\"Ignored: SMS is not needed for smartphones. Pass force=true if you want to actually send it anyway.\",\"cellphone\":\"+1-XXX-XXX-XX02\",\"device\":\"android\",\"ignored\":true,\"success\":true}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.TOTP, true));
        }
        #endregion TOTP


        #region VOICE
        [Fact]
        public async Task TestCreateVerificationVoiceNoForceNoAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true,\"message\":\"Call started...\",\"cellphone\":\"+1-XXX-XXX-XX02\"}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.VOICE, false));
        }

        [Fact]
        public async Task TestCreateVerificationVoiceForceNoAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true,\"message\":\"Call started...\",\"cellphone\":\"+1-XXX-XXX-XX02\"}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.VOICE, true));
        }

        [Fact]
        public async Task TestCreateVerificationVoiceNoForceAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"message\":\"Call ignored. User is using  App Tokens and this call is not necessary. Pass force=true if you still want to call users that are using the App.\",\"cellphone\":\"+1-XXX-XXX-XX02\",\"device\":\"android\",\"ignored\":true,\"success\":true}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.VOICE, false));
        }

        [Fact]
        public async Task TestCreateVerificationVoiceForceAuthy()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true,\"message\":\"Call started...\",\"cellphone\":\"+1-XXX-XXX-XX02\"}")
            };
            var authyClient = CreateClient(res);
            Assert.True(await authyClient.CreateVerification(userManager.Object, testUser, VerificationType.VOICE, true));
        }
        #endregion VOICE

        #region PUSH

        [Fact]
        public async Task TestCreateVerificationPushForce()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var authyClient = CreateClient(res);
            await Assert.ThrowsAsync<InvalidOperationException>(() => authyClient.CreateVerification(userManager.Object, testUser, VerificationType.PUSH, true));
        }

        [Fact]
        public async Task TestCreateVerificationPushNoForce()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var authyClient = CreateClient(res);
            await Assert.ThrowsAsync<InvalidOperationException>(() => authyClient.CreateVerification(userManager.Object, testUser, VerificationType.PUSH, false));
        }

        [Fact]
        public async Task TestCreateOneTouchPushNoUser()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            var authyClient = CreateClient(res);
            Assert.Null(await authyClient.CreateOneTouchPush(userManager.Object, new IdentityUser(), null));
        }

        [Fact]
        public async Task TestCreateOneTouchPushNoData()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"approval_request\": {\"uuid\": \"c31f7620-9726-0135-6e6f-0ad8af7cead6\"},\"success\": true}")
            };

            var authyClient = CreateClient(res);
            Assert.Equal("c31f7620-9726-0135-6e6f-0ad8af7cead6", await authyClient.CreateOneTouchPush(userManager.Object, testUser, new AuthyOneTouchDetails()));
        }

        [Fact]
        public async Task TestCreateOneTouchPushData()
        {
            var res = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"approval_request\": {\"uuid\": \"c31f7620-9726-0135-6e6f-0ad8af7cead6\"},\"success\": true}")
            };

            var authyClient = CreateClient(res);
            Assert.Equal("c31f7620-9726-0135-6e6f-0ad8af7cead6", await authyClient.CreateOneTouchPush(userManager.Object, testUser, new AuthyOneTouchDetails(true)
            {
                Message = "TestMessage",
                Details =
                {
                    { "IP", "FakeIPHere" }
                },
                HiddenDetails =
                {
                    { "Hidden", "Value" }
                },
                Logos =
                {
                    { "logo1", "value" }
                },
                SecondsToExpire = 300
            }));
        }
        #endregion PUSH
    }
}
