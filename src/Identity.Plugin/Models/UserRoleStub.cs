using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Models
{
    public class UserRoleStub
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
        public string RoleNormalizedName { get; set; }
        
        public static implicit operator IdentityRole(UserRoleStub x)
        {
            return new IdentityRole(x.RoleName) {NormalizedName = x.RoleNormalizedName,Id = x.Id};
        }
    }
}