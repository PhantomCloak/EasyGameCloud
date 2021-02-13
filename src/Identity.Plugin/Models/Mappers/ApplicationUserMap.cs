using Dapper.FluentMap.Mapping;

namespace Identity.Plugin.Models.Mappers
{
    public class ApplicationUserMap : EntityMap<ApplicationUser>
    {
        public  ApplicationUserMap()
        {
            Map(p => p.Id).ToColumn("id");
            Map(p => p.UserName).ToColumn("user_name");
            Map(p => p.PasswordHash).ToColumn("password_hash");
            Map(p => p.NormalizedUserName).ToColumn("normalized_username");
            Map(p => p.Email).ToColumn("email");
            Map(p => p.NormalizedEmail).ToColumn("normalized_email");
            Map(p => p.IsEmailVerified).ToColumn("email_verified");
            Map(p => p.PhoneNumber).ToColumn("phone_number");
            Map(p => p.IsPhoneNumberVerified).ToColumn("phone_verified");
            Map(p => p.SecurityStamp).ToColumn("security_stamp");
            Map(p => p.IsTwoFactorEnabled).ToColumn("two_factor_enabled");
            Map(p => p.AccessFailedCount).ToColumn("access_failed_count");
            Map(p => p.IsLockoutEnabled).ToColumn("lockout_enabled");
            Map(p => p.LockoutEndDate).ToColumn("lockout_end");
            Map(p => p.CreatedOn).ToColumn("created_on");
            Map(p => p.DeletedOn).ToColumn("deleted_on");
        }
    }
}