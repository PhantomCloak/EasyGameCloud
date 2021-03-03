using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Identity.Plugin.Models;
using Identity.Plugin.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Stores
{
    public class CustomUserStore<TUser> :
        IUserStore<TUser>,
        IUserLoginStore<TUser>,
        IUserRoleStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IProtectedUserStore<TUser>
        where TUser : ApplicationUser
    {
        private readonly IIdentityUserRepository<TUser> _userRepository;
        public IdentityOptions Options { get; set; }

        public CustomUserStore(
            IIdentityUserRepository<TUser> userRepository, IdentityOptions options)
        {
            _userRepository = userRepository;
            Options = options;
        }

        public void Dispose()
        {
        }

        public async Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            if (!string.IsNullOrEmpty(user.Id))
                return user.Id;

            var usr = await _userRepository.GetUserFromUsernameAsync(user.NormalizedUserName);

            return usr.Id;
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.UserName = user.UserName;
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var succeed = await _userRepository.CreateUserAsync(user)
                ? IdentityResult.Success
                : IdentityResult.Failed();

            return succeed;
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var succeed = await _userRepository.UpdateUserAsync(user)
                ? IdentityResult.Success
                : IdentityResult.Failed();

            return succeed;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var succeed = await _userRepository.DeleteUserAsync(user.Id)
                ? IdentityResult.Success
                : IdentityResult.Failed();

            return succeed;
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (userId == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var user = await _userRepository.GetUserFromIdAsync(userId);

            return user;
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (normalizedUserName == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var user = await _userRepository.GetUserFromUsernameAsync(normalizedUserName);

            return user;
        }

        public async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            throw new NotImplementedException();
            // await _userLoginRepository.CreateUserLogin(user, login);
        }

        public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            throw new NotImplementedException();
            // await _userLoginRepository.DeleteUserLogin(user, loginProvider, providerKey);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            throw new NotImplementedException();
            // return (await _userLoginRepository.GetUserLogin(user)).ToList();
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(loginProvider) || string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("loginProvider or providerKey parameters is null or incorrect.");
            }

            // return await _userLoginRepository.GetUserFromLogin(loginProvider, providerKey);
            throw new NotImplementedException();
        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("The roleName parameter is null or incorrect.");
            }

            await _userRepository.AddRoleToUserAsync(user, new IdentityRole(roleName));
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("The roleName parameter is null or incorrect.");
            }

            await _userRepository.RemoveRoleFromUser(user, roleName);
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var roles = await _userRepository.GetUserRolesAsync(user);

            return roles.Select(x => x.Name).ToList();
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException("The roleName parameter is null or incorrect");
            }

            var roles = await _userRepository.GetUserRolesAsync(user);

            return roles.Any(x => x.Name == roleName);
        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException("The roleName parameter is null or incorrect");
            }

            var users = await _userRepository.GetUsersFromRole(roleName);
            return users.ToList();
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return (await _userRepository.GetUserClaims(user)).ToList();
        }

        public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            await _userRepository.AddClaimsToUser(user, claims);
        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            await _userRepository.RemoveClaimFromUser(user, claim.Type);
            await _userRepository.AddClaimsToUser(user, new[] {newClaim});
        }

        public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            foreach (var claim in claims)
            {
                await _userRepository.RemoveClaimFromUser(user, claim.Type);
            }
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (claim == null)
            {
                throw new ArgumentNullException("Claim can't be null.");
            }

            var users = await _userRepository.GetUsersFromClaim(claim);

            return users.ToList();
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public async Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var usr = await _userRepository.GetUserFromIdAsync(user.Id);

            return usr.PasswordHash;
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public async Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            if (!string.IsNullOrEmpty(user.SecurityStamp))
                return user.SecurityStamp;

            var actor = await _userRepository.GetUserFromEmail(user.Email);

            return actor.SecurityStamp;
        }

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult<string>(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(user.IsEmailVerified);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.IsEmailVerified = confirmed;
            return Task.CompletedTask;
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(normalizedEmail))
            {
                throw new ArgumentNullException("NormalizedEmail can't be null");
            }

            var user = _userRepository.GetUserFromEmail(normalizedEmail);
            return user;
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(user.LockoutEndDate);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.LockoutEndDate = lockoutEnd;
            return Task.CompletedTask;
        }

        public async Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var usr = await _userRepository.GetUserFromIdAsync(user.Id);
            usr.AccessFailedCount++;

            if (!string.IsNullOrEmpty(user.Id))
                await _userRepository.UpdateUserAsync(usr);

            return usr.AccessFailedCount;
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public async Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var usr = await _userRepository.GetUserFromIdAsync(user.Id);

            return usr.AccessFailedCount;
        }

        public async Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            var usr = await _userRepository.GetUserFromIdAsync(user.Id);

            return usr.IsLockoutEnabled;
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.IsLockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            return Task.FromResult(user.IsPhoneNumberVerified);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException("The user parameter is null or incorrect.");
            }

            user.IsPhoneNumberVerified = confirmed;
            return Task.CompletedTask;
        }
    }
}