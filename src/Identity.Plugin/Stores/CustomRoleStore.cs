using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Identity.Plugin.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Stores
{
    public class CustomRoleStore : IRoleStore<IdentityRole>,IRoleClaimStore<IdentityRole>
    {
        private readonly IIdentityRoleRepository<IdentityRole> _roleRepository;

        public CustomRoleStore(IIdentityRoleRepository<IdentityRole> roleRepository)
        {
            _roleRepository = roleRepository;
            Console.WriteLine();
        }
        
        public void Dispose()
        {
            
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await _roleRepository.CreateRoleAsync(role,null);
            
            return result ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await _roleRepository.DeleteRoleAsync(role.Name);
            var result = await _roleRepository.CreateRoleAsync(role,null);

            return result ? IdentityResult.Success : IdentityResult.Failed();   
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await _roleRepository.DeleteRoleAsync(role.Name);
            
            return result ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (!string.IsNullOrEmpty(role.Id))
                return role.Id;

            var resultRole = await _roleRepository.GetRoleByNameAsync(role.Name);
            return resultRole.Id;
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (!string.IsNullOrEmpty(role.Name))
                return Task.FromResult(role.Name);

            return Task.FromResult(string.Empty);
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.FromResult("ok");
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (!string.IsNullOrEmpty(role.NormalizedName))
                return Task.FromResult(role.NormalizedName);

            return Task.FromResult(string.Empty);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            role.NormalizedName = normalizedName;
            return Task.FromResult("ok");
        }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var role = await _roleRepository.GetRoleByNameAsync(roleId);

            return role;
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleRepository.GetRoleByNameAsync(normalizedRoleName);

            return role;
        }

        public async Task<IList<Claim>> GetClaimsAsync(IdentityRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var roleClaims = await _roleRepository.GetRoleClaims(role.Name);

            return roleClaims.ToList();
        }

        public async Task AddClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await _roleRepository.AddRoleClaims(role, new[] {claim});
        }

        public async Task RemoveClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await _roleRepository.RemoveRoleClaims(role, claim.Type);
        }
    }
}