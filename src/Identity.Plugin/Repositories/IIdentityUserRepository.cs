using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Repositories
{
    public interface IIdentityUserRepository<TUser>
    {
        Task<TUser> GetUserFromIdAsync(string id);
        Task<TUser> GetUserFromUsernameAsync(string userName);
        Task<TUser> GetUserFromEmail(string email);
        Task<IEnumerable<TUser>> GetUsersAsync();
        Task<bool> CreateUserAsync(TUser user);
        Task<bool> UpdateUserAsync(TUser user);
        Task<bool> DeleteUserAsync(string id);

        Task<IEnumerable<TUser>> GetUsersFromClaim(Claim claim);
        Task<IEnumerable<TUser>> GetUsersFromRole(string roleName);
        
        Task<IEnumerable<IdentityRole>> GetUserRolesAsync(TUser user);
        Task<IEnumerable<Claim>> GetUserClaims(TUser user);

        Task AddRoleToUserAsync(TUser user,IdentityRole role);
        Task RemoveRoleFromUser(TUser user, string roleName);
        
        Task AddClaimsToUser(TUser user,IEnumerable<Claim> claims);
        Task RemoveClaimFromUser(TUser user, string claimType);
    }
}