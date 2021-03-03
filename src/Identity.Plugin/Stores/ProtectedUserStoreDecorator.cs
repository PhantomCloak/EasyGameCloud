using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Identity.Plugin.Models;
using Identity.Plugin.Repositories;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Serialization;

namespace Identity.Plugin.Stores
{
    public class ProtectedUserStoreDecorator<TUser> :
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
        private readonly IUserStore<TUser> _store;
        private readonly IIdentityUserKeyRepository _keyRepository;
        private readonly ILookupProtectorKeyRing _keyRing;
        private readonly ILookupProtector _protector;

        public ProtectedUserStoreDecorator(IUserStore<TUser> store,
            IIdentityUserKeyRepository keyRepository,
            ILookupProtectorKeyRing keyRing,
            ILookupProtector protector)
        {
            _store = store;
            _keyRepository = keyRepository;
            _keyRing = keyRing;
            _protector = protector;
        }

        public void Dispose()
        {
            _store.Dispose();
        }

        public async Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            return await _store.GetUserIdAsync(user, cancellationToken);
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return _store.GetUserNameAsync(user, cancellationToken);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            return _store.SetUserNameAsync(user, userName, cancellationToken);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return _store.GetNormalizedUserNameAsync(user, cancellationToken);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            return _store.SetUserNameAsync(user, normalizedName, cancellationToken);
        }

        public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            return _store.CreateAsync(user, cancellationToken);
        }

        public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            var key = _keyRing.GetAllKeyIds().Random();

            user.UserName = _protector.Protect(key, user.UserName);
            user.Email = _protector.Protect(key, user.Email);
            
            return _store.UpdateAsync(user, cancellationToken);
        }

        public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            return _store.DeleteAsync(user, cancellationToken);
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return _store.FindByIdAsync(userId, cancellationToken);
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var result = await _store.FindByNameAsync(normalizedUserName, cancellationToken);
            var userKeyId = await _keyRepository.GetKeyIdForUser(result.Id);
            var contains = _keyRing.GetAllKeyIds().Contains(userKeyId);
            var userKey = _keyRing[userKeyId];
            
            if (!contains)
            {
                throw new KeyNotFoundException();
            }

            result.Email = _protector.Unprotect(userKey, result.Email);
            result.UserName = _protector.Unprotect(userKey, result.UserName);
            result.PhoneNumber = _protector.Unprotect(userKey, result.PhoneNumber);
            
            return result;
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            var cast = _store as IUserLoginStore<TUser>;
            return cast.AddLoginAsync(user, login, cancellationToken);
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            if (!(_store is IUserLoginStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.RemoveLoginAsync(user, loginProvider, providerKey, cancellationToken);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLoginStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetLoginsAsync(user, cancellationToken);
        }

        public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            if (!(_store is IUserLoginStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.FindByLoginAsync(loginProvider, providerKey, cancellationToken);
        }

        public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (!(_store is IUserRoleStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.AddToRoleAsync(user, roleName, cancellationToken);
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (!(_store is IUserRoleStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.RemoveFromRoleAsync(user, roleName, cancellationToken);
        }

        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserRoleStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetRolesAsync(user, cancellationToken);
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (!(_store is IUserRoleStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.IsInRoleAsync(user, roleName, cancellationToken);
        }

        public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (!(_store is IUserRoleStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetUsersInRoleAsync(roleName, cancellationToken);
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserClaimStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetClaimsAsync(user, cancellationToken);
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (!(_store is IUserClaimStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.AddClaimsAsync(user, claims, cancellationToken);
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (!(_store is IUserClaimStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.ReplaceClaimAsync(user, claim, newClaim, cancellationToken);
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (!(_store is IUserClaimStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.RemoveClaimsAsync(user, claims, cancellationToken);
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            if (!(_store is IUserClaimStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetUsersForClaimAsync(claim, cancellationToken);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            if (!(_store is IUserPasswordStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetPasswordHashAsync(user, passwordHash, cancellationToken);
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserPasswordStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetPasswordHashAsync(user, cancellationToken);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserPasswordStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.HasPasswordAsync(user, cancellationToken);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            if (!(_store is IUserSecurityStampStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetSecurityStampAsync(user, stamp, cancellationToken);
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserSecurityStampStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetSecurityStampAsync(user, cancellationToken);
        }

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            if (!(_store is IUserEmailStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetEmailAsync(user, email, cancellationToken);
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserEmailStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetEmailAsync(user, cancellationToken);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserEmailStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetEmailConfirmedAsync(user, cancellationToken);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (!(_store is IUserEmailStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetEmailConfirmedAsync(user, confirmed, cancellationToken);
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            if (!(_store is IUserEmailStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.FindByEmailAsync(normalizedEmail, cancellationToken);
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserEmailStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetNormalizedEmailAsync(user, cancellationToken);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (!(_store is IUserEmailStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetNormalizedEmailAsync(user,normalizedEmail, cancellationToken);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLockoutStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetLockoutEndDateAsync(user, cancellationToken);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLockoutStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetLockoutEndDateAsync(user,lockoutEnd, cancellationToken);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLockoutStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.IncrementAccessFailedCountAsync(user, cancellationToken);
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLockoutStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.ResetAccessFailedCountAsync(user, cancellationToken);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLockoutStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetAccessFailedCountAsync(user, cancellationToken);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLockoutStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetLockoutEnabledAsync(user, cancellationToken);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (!(_store is IUserLockoutStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetLockoutEnabledAsync(user,enabled, cancellationToken);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            if (!(_store is IUserPhoneNumberStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetPhoneNumberAsync(user,phoneNumber, cancellationToken);
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserPhoneNumberStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetPhoneNumberAsync(user, cancellationToken);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            if (!(_store is IUserPhoneNumberStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.GetPhoneNumberConfirmedAsync(user, cancellationToken);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (!(_store is IUserPhoneNumberStore<TUser> cast))
            {
                throw new InvalidCastException();
            }

            return cast.SetPhoneNumberConfirmedAsync(user,confirmed, cancellationToken);
        }
    }
}