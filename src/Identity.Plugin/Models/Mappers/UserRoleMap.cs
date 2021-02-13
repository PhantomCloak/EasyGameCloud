using Dapper.FluentMap.Mapping;

namespace Identity.Plugin.Models.Mappers
{
    public class UserRoleMap : EntityMap<UserRoleStub>
    {
        public UserRoleMap()
        {
            Map(p => p.Id).ToColumn("role_id");
            Map(p => p.RoleName).ToColumn("role_name");
            Map(p => p.RoleNormalizedName).ToColumn("role_normalized_name");
        }
    }
}