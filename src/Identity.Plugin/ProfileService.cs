using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Plugin.Models;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin
{
    public class ProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        
        public ProfileService(UserManager<ApplicationUser> userManager,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _roleManager = roleManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var userClaims = await _claimsFactory.CreateAsync(user);
            var claims = userClaims.Claims.ToList();

            if (_userManager.SupportsUserRole)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var roleName in roles)
                {
                    claims.Add(new Claim(JwtClaimTypes.Role,roleName));
                    if (_roleManager.SupportsRoleClaims)
                    {
                        IdentityRole role = await _roleManager.FindByNameAsync(roleName);
                    
                        if (role != null)
                        {
                            claims.AddRange(await _roleManager.GetClaimsAsync(role));
                        }
                    }
                }
            }
            
            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}