using System.Security.Claims;

namespace Identity.Plugin.Models
{
    public class UserClaimStub 
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        
        public static implicit operator Claim(UserClaimStub x)
        {
            return new Claim(x.ClaimType, x.ClaimValue);
        }
    }
}