using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Identity.Plugin.Repositories
{
    public class IdentityRoleRepository : IIdentityRoleRepository<IdentityRole>
    {
        private readonly string _connectionStr;

        public IdentityRoleRepository(IConfiguration configuration)
        {
            _connectionStr = configuration["Database:ConnectionString"];
        }

        public async Task<IEnumerable<IdentityRole>> GetRolesAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionStr);
            await connection.OpenAsync();

            var roleStubs =
                await connection.QueryAsync<UserRoleStub>(
                    @"select role_id,role_name,role_normalized_name from identity_role");

            //needs cast
            var roles = roleStubs.Select(role => (IdentityRole) role).ToList();

            await connection.CloseAsync();
            return roles;
        }

        public async Task<IdentityRole> GetRoleByNameAsync(string roleName)
        {
            await using var connection = new NpgsqlConnection(_connectionStr);
            await connection.OpenAsync();

            var roleStubs = await connection.QueryAsync<UserRoleStub>(
                @"select role_id,role_name,role_normalized_name from identity_role where role_normalized_name= @rolename",
                new {rolename = roleName});
            var roleStub = roleStubs.FirstOrDefault();

            await connection.CloseAsync();
            return roleStub;
        }

        public async Task<bool> CreateRoleAsync(IdentityRole role, IEnumerable<Claim> claims)
        {
            await using var connection = new NpgsqlConnection(_connectionStr);
            await connection.OpenAsync();

            var createRoleParams = new DynamicParameters();

            createRoleParams.Add("@id", role.Id);
            createRoleParams.Add("@roleName", role.Name);
            createRoleParams.Add("@roleNormalizedName", role.NormalizedName);

            await connection.ExecuteAsync(@"insert into identity_role values(@id,@roleName,@roleNormalizedName)",
                createRoleParams);

            if (claims != null)
                foreach (var claim in claims)
                {
                    var createClaimParams = new DynamicParameters();
                    
                    createClaimParams.Add("@roleId",role.Id);
                    createClaimParams.Add("@claimType",claim.Type);
                    createClaimParams.Add("@claimValue",claim.Value);

                    await connection.ExecuteAsync(
                        @"insert into identity_role_claim VALUES(default,@claimType,@claimValue,@roleId)",createClaimParams);
                }

            await connection.CloseAsync();

            return true;
        }

        public async Task<bool> RemoveRoleClaims(IdentityRole role, string claimType)
        {
            await using var connection = new NpgsqlConnection(_connectionStr);
            await connection.OpenAsync();
            
            var createClaimParams = new DynamicParameters();
                    
            createClaimParams.Add("@roleId",role.Id);
            createClaimParams.Add("@claimType",claimType);
            
            await connection.ExecuteAsync(@"delete from identity_role_claim where role_id=@roleId and claim_type=@claimType");

            return true;
        }

        public async Task<IEnumerable<Claim>> GetRoleClaims(string roleName)
        {
            await using var connection = new NpgsqlConnection(_connectionStr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@roleName", roleName);

            var claimStubs = await connection.QueryAsync<UserClaimStub>(
                @"select claim_type,claim_value from identity_role left join identity_role_claim irc on identity_role.role_id = irc.role_id where role_name = @roleName",
                parameters);

            var roles = claimStubs.Select(role => (Claim) role).ToList();

            return roles;
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            await using var connection = new NpgsqlConnection(_connectionStr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@roleId", roleId);

            await connection.ExecuteAsync(@"delete from identity_role where role_id=@roleId", parameters);

            return true;
        }

        public async Task<bool> AddRoleClaims(IdentityRole role, IEnumerable<Claim> claims)
        {
            await using var connection = new NpgsqlConnection(_connectionStr);
            await connection.OpenAsync();
   
            foreach (var claim in claims)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleId",role.Id);
                parameters.Add("@claimType",claim.Type);
                parameters.Add("@claimValue",claim.Value);

                await connection.ExecuteAsync(@"insert into identity_role_claim VALUES(DEFAULT,@claimType,@claimValue,@roleId)", parameters);
            }

            return true;
        }
    }
}