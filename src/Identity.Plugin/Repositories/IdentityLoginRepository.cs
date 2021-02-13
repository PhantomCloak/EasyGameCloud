using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Identity.Plugin.Repositories
{
    public class IdentityUserLoginRepository : IIdentityUserLoginRepository<ApplicationUser>
    {
        private readonly IIdentityUserRepository<ApplicationUser> _userRepository;

        private const string strrr =
            "Server=localhost;" +
            "Port=5432;" +
            "Database=postgres;" +
            "User Id=postgres;" +
            "Password=badf00d11";

        public IdentityUserLoginRepository(IIdentityUserRepository<ApplicationUser> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserLoginInfo>> GetUserLogin(ApplicationUser user)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var logins = await connection.QueryAsync<UserLoginStub>(@"select * from identity_user_logins
             where user_id=@userId", new {userId = user.Id});

            //needs cast
            var actualLogins = logins.Select(login => (UserLoginInfo) login).ToList();

            await connection.CloseAsync();
            return actualLogins;
        }

        public async Task<ApplicationUser> GetUserFromLogin(string providerName, string providerKey)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var logins = await connection.QueryAsync<string>(@"select user_id from identity_user_logins
             where provider=@providername and provider_key= @providerkey",
                new {providername = providerName, providerkey = providerKey});

            //needs cast
            var userId = logins.First();

            var user = await _userRepository.GetUserFromIdAsync(userId);

            await connection.CloseAsync();
            return user;
        }

        public async Task<bool> CreateUserLogin(ApplicationUser user, UserLoginInfo loginInfo)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@provider", loginInfo.LoginProvider);
            parameters.Add("@providerKey", loginInfo.ProviderKey);
            parameters.Add("@providerDisplayName", loginInfo.ProviderDisplayName);
            parameters.Add("@userId", user.Id);

            await connection.ExecuteAsync(
                "insert into identity_user_logins VALUES(@provider,@providerKey,@providerDisplayName,@userId)",
                new
                {
                    provider = loginInfo.LoginProvider,
                    providerKey = loginInfo.ProviderKey,
                    providerDisplayName = loginInfo.ProviderDisplayName,
                    userId = user.Id
                });

            return true;
        }

        public async Task<bool> UpdateUserLogin(ApplicationUser user, UserLoginInfo loginInfo)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                @"update identity_user_logins set 
                 provider= @provider,
                 provider_key= @providerKey,
                 provider_display_name= @provider_display_name,
                 user_id= @userId",
                new
                {
                    provider = loginInfo.LoginProvider, providerKey = loginInfo.ProviderKey,
                    provider_display_name = loginInfo.ProviderDisplayName, userId = user.Id
                });

            return true;
        }

        public async Task<bool> DeleteUserLogin(ApplicationUser user, string providerName, string providerKey)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "delete from identity_user_logins where provider=@providername and provider_key=@providerkey and user_id=@userId",
                new {providername = providerName, providerkey = providerKey, userId = user.Id});

            return true;
        }

        public async Task<bool> DeleteUserLogins(ApplicationUser user)
        {
            await using var connection = new NpgsqlConnection(strrr);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "delete from identity_user_logins where user_id=@userId",
                new {userId = user.Id});

            return true;
        }
    }
}