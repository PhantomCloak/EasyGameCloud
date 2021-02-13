using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Repositories
{
    public interface IIdentityRoleRepository<T>
    {
        Task<IEnumerable<T>> GetRolesAsync();
        Task<T> GetRoleByNameAsync(string roleName);
        Task<bool> CreateRoleAsync(IdentityRole role, IEnumerable<Claim> claims);
        Task<bool> DeleteRoleAsync(string roleId);

        Task<bool> AddRoleClaims(IdentityRole role, IEnumerable<Claim> claims);
        Task<bool> RemoveRoleClaims(IdentityRole role, string claimType);
        Task<IEnumerable<Claim>> GetRoleClaims(string roleName);
    }
}