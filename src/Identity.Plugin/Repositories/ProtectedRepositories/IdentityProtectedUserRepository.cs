using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Repositories.ProtectedRepositories
{
    public class IdentityProtectedUserRepository : IIdentityUserRepository<ApplicationUser>
    {
        private readonly IIdentityUserRepository<ApplicationUser> _instance;
        private readonly IPersonalDataProtector _personalDataProtector;

        public IdentityProtectedUserRepository(
            IIdentityUserRepository<ApplicationUser> userRepository,
            IPersonalDataProtector personalDataProtector)
        {
            _instance = userRepository;
            _personalDataProtector = personalDataProtector;
        }

        public async Task<ApplicationUser> GetUserFromIdAsync(string id)
        {
            var result = await _instance.GetUserFromIdAsync(id);

            result.Email = _personalDataProtector.Unprotect(result.Email);
            result.UserName = _personalDataProtector.Unprotect(result.UserName);

            return result;
        }

        public async Task<ApplicationUser> GetUserFromUsernameAsync(string userName)
        {
            var result = await _instance.GetUserFromUsernameAsync(userName);

            result.Email = _personalDataProtector.Unprotect(result.Email);
            result.UserName = _personalDataProtector.Unprotect(result.UserName);

            return result;
        }

        public async Task<ApplicationUser> GetUserFromEmail(string email)
        {
            var result = await _instance.GetUserFromEmail(email);

            if (result == null)
            {
                return null;
            }
            
            result.Email = _personalDataProtector.Unprotect(result.Email);
            result.UserName = _personalDataProtector.Unprotect(result.UserName);

            return result;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            var result = await _instance.GetUsersAsync();

            foreach (var user in result)
            {
                user.Email = _personalDataProtector.Unprotect(user.Email);
                user.UserName = _personalDataProtector.Unprotect(user.UserName);
            }

            return result;
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user)
        {
            if (user == null)
                return false;

            // we create snapshot of this object due to parameter passing by ref
            var snapshot = (ApplicationUser) user.Clone();

            snapshot.Email = _personalDataProtector.Protect(user.Email);
            snapshot.UserName = _personalDataProtector.Protect(user.UserName);

            return await _instance.CreateUserAsync(snapshot);
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            if (user == null)
                return false;

            // we create snapshot of this object due to parameter passing by ref
            var snapshot = (ApplicationUser) user.Clone();
            
            snapshot.Email = _personalDataProtector.Protect(snapshot.Email);
            snapshot.UserName = _personalDataProtector.Protect(snapshot.UserName);

            return await _instance.UpdateUserAsync(snapshot);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _instance.DeleteUserAsync(id);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersFromClaim(Claim claim)
        {
            var result = await _instance.GetUsersFromClaim(claim);

            foreach (var user in result)
            {
                user.Email = _personalDataProtector.Unprotect(user.Email);
                user.UserName = _personalDataProtector.Unprotect(user.UserName);
            }

            return result;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersFromRole(string roleName)
        {
            var result = await _instance.GetUsersFromRole(roleName);

            foreach (var user in result)
            {
                user.Email = _personalDataProtector.Unprotect(user.Email);
                user.UserName = _personalDataProtector.Unprotect(user.UserName);
            }

            return result;
        }

        public async Task<IEnumerable<IdentityRole>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _instance.GetUserRolesAsync(user);
        }

        public async Task<IEnumerable<Claim>> GetUserClaims(ApplicationUser user)
        {
            return await _instance.GetUserClaims(user);
        }

        public async Task AddRoleToUserAsync(ApplicationUser user, IdentityRole role)
        {
            await _instance.AddRoleToUserAsync(user, role);
        }

        public async Task RemoveRoleFromUser(ApplicationUser user, string roleName)
        {
            await _instance.RemoveRoleFromUser(user, roleName);
        }

        public async Task AddClaimsToUser(ApplicationUser user, IEnumerable<Claim> claims)
        {
            await _instance.AddClaimsToUser(user, claims);
        }

        public async Task RemoveClaimFromUser(ApplicationUser user, string claimType)
        {
            await _instance.RemoveClaimFromUser(user, claimType);
        }
    }
}