using System.Threading.Tasks;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers
{
    [Authorize]
    public class Endpoints
    {
        public UserManager<ApplicationUser> UserManager { get; }

        public Endpoints(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }
        
        [AllowAnonymous]
        public Task<IActionResult> LoginAccount()
        {
            
        }
    }
}