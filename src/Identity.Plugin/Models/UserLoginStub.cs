
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin.Models
{
    internal class UserLoginStub
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }

        public static implicit operator UserLoginInfo(UserLoginStub x)
        {
            return new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName);
        }
    }
}