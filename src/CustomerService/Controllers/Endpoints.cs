using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [Authorize]
    public class Endpoints
    {
        public Task<IActionResult> CreateCustomer(string userName)
        {
            
            return null;
        }
    }
}