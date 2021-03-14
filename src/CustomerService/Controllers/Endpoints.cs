using System.Threading.Tasks;
using CustomerService.Contracts.CommandModels;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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