using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Authy.AspNetCore
{
    public interface IAuthyCall2FA
    {
        Task<string> RegisterUserAsync<T>(AuthyUser authyUser, UserManager<T> manager, T user) where T : IdentityUser;
    }
}
