using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Repositories
{
    public interface IIdentityUserLoginRepository<T>
    {
        Task<IEnumerable<UserLoginInfo>> GetUserLogin(T user);
        Task<T> GetUserFromLogin(string providerName, string providerKey);
        Task<bool> CreateUserLogin(T user, UserLoginInfo loginInfo);
        Task<bool> DeleteUserLogin(T user,string providerName, string providerKey);
        Task<bool> DeleteUserLogins(T user);
    }
}