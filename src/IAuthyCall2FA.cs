using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Authy.AspNetCore
{
    public interface IAuthyCall2FA
    {
        Task<string> RegisterUserAsync<T>(AuthyUser authyUser, UserManager<T> manager, T user) where T : IdentityUser;

        Task<AuthyUser> GetUserAsync<T>(UserManager<T> manager, T user) where T : IdentityUser;

        Task<bool> RemoveUserAsync<T>(UserManager<T> manager, T user) where T : IdentityUser;

        Task<bool> CreateVerification<T>(UserManager<T> manager, T user, VerificationType verificationType, bool force = false) where T : IdentityUser;

        Task<string> CreatePushVerificaiton<T>(UserManager<T> manager, T user, AuthyPushNotificationDetails details) where T : IdentityUser;

        Task<bool> SetUserPreferredVerificationTypeAsync<T>(UserManager<T> manager, T user, VerificationType verificationType) where T : IdentityUser;

        Task<VerificationType> GetUserPreferredVerificationTypeAsync<T>(UserManager<T> manager, T user) where T : IdentityUser;
    }
}
