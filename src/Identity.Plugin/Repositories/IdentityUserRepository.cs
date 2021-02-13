using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Identity.Plugin.Repositories
{
  
    public class IdentityUserRepository : IIdentityUserRepository<ApplicationUser>
    {

        private const string strrr =
            "Server=localhost;" +
            "Port=5432;" +
            "Database=identity;" +
            "User Id=db_admin;" +
            "Password=badf00d11";

        public IdentityUserRepository()
        {
            // private readonly IIdentityRoleRepository<IdentityRole> _roleRepository;
            // _roleRepository = roleRepository;
        }

        public async Task<ApplicationUser> GetUserFromIdAsync(string userId)
        {
            var plot = PlotSelect(userId: userId);
            return (await GetUsersAsync(plot.QueryString, plot.Parameters)).FirstOrDefault();
        }

        public async Task<ApplicationUser> GetUserFromUsernameAsync(string userName)
        {
            var plot = PlotSelect(userName: userName);
            return (await GetUsersAsync(plot.QueryString, plot.Parameters)).FirstOrDefault();
        }

        public async Task<ApplicationUser> GetUserFromEmail(string email)
        {
            var plot = PlotSelect(email: email);
            return (await GetUsersAsync(plot.QueryString, plot.Parameters)).FirstOrDefault();
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            var plottedSql = PlotSelect().QueryString;
            return await GetUsersAsync(plottedSql, null);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersFromClaim(Claim claim)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var users =await connection.QueryAsync<ApplicationUser>(
                "select identity_user.* from identity_user left join identity_user_claim iuc on identity_user.id = iuc.user_id where claim_type = @claimtype",
                new {claimtype = claim.Type});
            
            return users;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersFromRole(string roleName)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var users = await connection.QueryAsync<ApplicationUser>(@"select *
            from identity_user
            left join identity_user_role iur on identity_user.id = iur.user_id
            left join identity_role  ir on iur.role_id = ir.role_id where role_name=@rolename",
                new {rolename = roleName});

            return users;
        }
        
        private async Task<IEnumerable<ApplicationUser>> GetUsersAsync(string plottedSql, object parameters)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var users = await connection.QueryAsync<ApplicationUser>(plottedSql, parameters);

            await connection.CloseAsync();
            return users;
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                @"insert into identity_user
                     values(DEFAULT,
                            @username,
                            @password_hash, 
                            @normalized_username,
                            @email,
                            @normalized_email,
                            @email_verified,
                            @phone_number,
                            @phone_verified,
                            @security_stamp,
                            @two_factor_enabled,
                            @access_failed_count,
                            @lockout_enabled,
                            @lockout_end,
                            @created_on,
                            @deleted_on)",
                new
                {
                    username = user.UserName,
                    password_hash = user.PasswordHash,
                    normalized_username = user.NormalizedUserName,
                    email = user.Email,
                    normalized_email = user.NormalizedEmail,
                    email_verified = user.IsEmailVerified,
                    phone_number = user.PhoneNumber,
                    phone_verified = user.IsPhoneNumberVerified,
                    security_stamp = user.SecurityStamp,
                    two_factor_enabled = user.IsTwoFactorEnabled,
                    access_failed_count = user.AccessFailedCount,
                    lockout_enabled = user.IsLockoutEnabled,
                    lockout_end = user.LockoutEndDate,
                    created_on = user.CreatedOn,
                    deleted_on = user.DeletedOn
                });
            await connection.CloseAsync();

            return true;
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();
            
            var parameters = new DynamicParameters();
            parameters.Add("@username",user.UserName);
            parameters.Add("@password_hash",user.PasswordHash);
            parameters.Add("@normalized_username",user.NormalizedUserName);
            parameters.Add("@email",user.Email);
            parameters.Add("@normalized_email",user.NormalizedEmail);
            parameters.Add("@email_verified",user.IsEmailVerified);
            parameters.Add("@phone_number",user.PhoneNumber);
            parameters.Add("@phone_verified",user.IsPhoneNumberVerified);
            parameters.Add("@security_stamp",user.SecurityStamp);
            parameters.Add("@two_factor_enabled",user.IsTwoFactorEnabled);
            parameters.Add("@access_failed_count",user.AccessFailedCount);
            parameters.Add("@lockout_enabled",user.IsLockoutEnabled);
            parameters.Add("@lockout_end",user.LockoutEndDate);
            parameters.Add("@created_on",user.CreatedOn);
            parameters.Add("@deleted_on",user.DeletedOn);
            parameters.Add("@userId",user.Id);

            await connection.ExecuteAsync(
                @"update identity_user set 
                         username= @username,
                         password_hash= @password_hash,
                         normalized_username= @normalized_username,
                         email= @email,
                         normalized_email = @normalized_email,
                         email_verified= @email_verified,
                         phone_number= @phone_number,
                         phone_verified= @phone_verified,
                         security_stamp= @security_stamp,
                         two_factor_enabled= @two_factor_enabled,
                         access_failed_count= @access_failed_count,
                         lockout_enabled= @lockout_enabled,
                         lockout_end= @lockout_end,
                         created_on= @created_on,
                         deleted_on= @deleted_on where id= @userId",parameters);

            await connection.CloseAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@userId",id);
            
            await connection.ExecuteAsync("DELETE FROM identity_user WHERE id=@userId",parameters);
            await connection.CloseAsync();
            return true;
        }

        public async Task<IEnumerable<IdentityRole>> GetUserRolesAsync(ApplicationUser user)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@userId",user.Id);

            var roleStubs = await connection.QueryAsync<UserRoleStub>(@"select ir.*
            from identity_user
            left join identity_user_role iur on identity_user.id = iur.user_id
            left join identity_role  ir on iur.role_id = ir.role_id where user_id=@userId", parameters);
            
            var roles = roleStubs.Select(role => (IdentityRole) role).ToList();
            return roles;
        }

        public async Task<IEnumerable<Claim>> GetUserClaims(ApplicationUser user)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@userId",user.Id);

            var claimStubs = await connection.QueryAsync<UserClaimStub>(@"select claim_type,claim_value from identity_user_claim
                        left join identity_user iu on iu.id = identity_user_claim.user_id where user_id=@userId", parameters);
            
            var roles = claimStubs.Select(role => (Claim) role).ToList();
            return roles;
        }

        public async Task AddRoleToUserAsync(ApplicationUser user, IdentityRole role)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@userId",user.Id);
            parameters.Add("@roleId",role.Id);

            await connection.ExecuteAsync(@"INSERT INTO identity_user_role VALUES(DEFAULT,@roleId,@userId);", parameters);
        }

        public async Task RemoveRoleFromUser(ApplicationUser user, string roleName)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            // var role = await _roleRepository.GetRoleByNameAsync(roleName);
            var role = new ApplicationUser();
            var parameters = new DynamicParameters();
            parameters.Add("@userId",user.Id);
            parameters.Add("@roleId",role.Id);

            await connection.ExecuteAsync(@"DELETE FROM identity_user_role WHERE user_id = @userId AND role_id= @roleId;", parameters);
        }

        public async Task AddClaimsToUser(ApplicationUser user, IEnumerable<Claim> claims)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var userClaims = await GetUserClaims(user);
            var claimsToAdd = claims.Except(userClaims).ToList();
            
            foreach (var claim in claimsToAdd)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId",user.Id);
                parameters.Add("@claimType",claim.Type);
                parameters.Add("@claimValue",claim.Value);
                
                await connection.ExecuteAsync(@"INSERT INTO identity_user_claim VALUES(default,@userId,@claimType,@claimValue)", parameters);
            }
        }

        public async Task RemoveClaimFromUser(ApplicationUser user, string claimType)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@userId",user.Id);
            parameters.Add("@claimType",claimType);

            await connection.ExecuteAsync(@"DELETE FROM identity_user_claim WHERE user_id=@userId AND claim_type=@claimType", parameters);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersFromClaim(string claimType)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var users = await connection.QueryAsync<ApplicationUser>(@"select identity_user.* from identity_user
             left join identity_user_claim iuc on identity_user.id = iuc.user_id where claim_type = @claimType",
                new {claimType});

            await connection.CloseAsync();
            return users.ToList();
        }

        private (string QueryString, object Parameters) PlotSelect(string userId = null, string userName = null,
            string email = null)
        {
            var sqlBuilder = new SqlBuilder();

            var selector = sqlBuilder.AddTemplate(@"select * from identity_user /**where**/");

            if (!string.IsNullOrEmpty(userId))
                sqlBuilder.Where("id= @userId", new
                {
                    userId
                });

            if (!string.IsNullOrEmpty(userName))
                sqlBuilder.Where("normalized_username= @userName", new
                {
                    userName
                });

            if (!string.IsNullOrEmpty(email))
                sqlBuilder.Where("normalized_email= @email", new
                {
                    email
                });

            return (selector.RawSql, selector.Parameters);
        }
    }
}