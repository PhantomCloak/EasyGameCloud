using Dapper.FluentMap.Mapping;

namespace Identity.Plugin.Models.Mappers
{
    public class UserClaimMap : EntityMap<UserClaimStub>
    {
        public UserClaimMap()
        {
            Map(x => x.ClaimType).ToColumn("claim_type");
            Map(x => x.ClaimValue).ToColumn("claim_value");
        }
    }
}