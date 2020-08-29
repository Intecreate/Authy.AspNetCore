using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Authy.AspNetCore
{
    public interface IAuthyClient
    {
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <typeparam name="T">The IdentityUser</typeparam>
        /// <param name="authyUser">The user details sent to authy</param>
        /// <param name="manager">The usermanager of your app</param>
        /// <param name="user">The user that you want to add authy to</param>
        /// <returns>The user id of the user on success, or null on failure</returns>
        Task<string> RegisterUserAsync<T>(AuthyUser authyUser, UserManager<T> manager, T user) where T : IdentityUser;

        /// <summary>
        /// Gets a users details from Authy (THROWS AN EXCEPTION, NOT DONE)
        /// </summary>
        /// <typeparam name="T">The IdentityUser</typeparam>
        /// <param name="manager">The usermanager of your app</param>
        /// <param name="user">The user that you want to get the user data for</param>
        /// <returns>The data that authy has about the user</returns>
        Task<AuthyUser> GetUserAsync<T>(UserManager<T> manager, T user) where T : IdentityUser;

        /// <summary>
        /// Removes a user from Authy
        /// </summary>
        /// <typeparam name="T">The IdentityUser</typeparam>
        /// <param name="manager">The usermanager of your app</param>
        /// <param name="user">The user that you want to remove from authy</param>
        /// <returns>If removing the user was successful or not</returns>
        Task<bool> RemoveUserAsync<T>(UserManager<T> manager, T user) where T : IdentityUser;

        /// <summary>
        /// Creates a notification to the user
        /// </summary>
        /// <typeparam name="T">The IdentityUser</typeparam>
        /// <param name="manager">The usermanager of your app</param>
        /// <param name="user">The user you are creating the notification for</param>
        /// <param name="verificationType">The type of notification you want to send</param>
        /// <param name="force">Details if the notification should be forced (adds force param)</param>
        /// <returns></returns>
        Task<bool> CreateVerification<T>(UserManager<T> manager, T user, VerificationType verificationType, bool force = false) where T : IdentityUser;

        /// <summary>
        /// Creates an OneTouch push notification
        /// </summary>
        /// <typeparam name="T">The IdentityUser</typeparam>
        /// <param name="manager">The usermanager of your app</param>
        /// <param name="user">The user you are creating the OneTouch notification for</param>
        /// <param name="details">Any details about the notification that you want to send to the user</param>
        /// <returns>The approval request uuid</returns>
        Task<string> CreateOneTouchPush<T>(UserManager<T> manager, T user, AuthyOneTouchDetails details) where T : IdentityUser;

        /// <summary>
        /// Allows you to set a users preferred verification type
        /// </summary>
        /// <typeparam name="T">The IdentityUser</typeparam>
        /// <param name="manager">The usermanager of your app</param>
        /// <param name="user">The user you are creating setting the preferred verification for</param>
        /// <param name="verificationType">The verificationtype the user preferres</param>
        /// <returns>If the verificationtype was set successfully</returns>
        Task<bool> SetUserPreferredVerificationTypeAsync<T>(UserManager<T> manager, T user, VerificationType verificationType) where T : IdentityUser;

        /// <summary>
        /// Gets the preferred verification type of a user
        /// </summary>
        /// <typeparam name="T">The IdentityUser</typeparam>
        /// <param name="manager">The usermanager of your app</param>
        /// <param name="user">The user you are trying to get the preferred verification type</param>
        /// <returns>The preferred vericication type of the user</returns>
        Task<VerificationType> GetUserPreferredVerificationTypeAsync<T>(UserManager<T> manager, T user) where T : IdentityUser;
    }
}
