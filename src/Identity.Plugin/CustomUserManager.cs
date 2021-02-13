// using System;
// using System.Collections.Generic;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
//
// namespace Identity.Plugin
// {
//     public class CustomUserManager<T> : UserManager<T> where T : class
//     {
//         private readonly IServiceProvider _services;
//
//         public CustomUserManager(
//             IUserStore<T> store,
//             IOptions<IdentityOptions> optionsAccessor,
//             IPasswordHasher<T> passwordHasher,
//             IEnumerable<IUserValidator<T>> userValidators,
//             IEnumerable<IPasswordValidator<T>> passwordValidators,
//             ILookupNormalizer keyNormalizer,
//             IdentityErrorDescriber errors,
//             IServiceProvider services,
//             ILogger<UserManager<T>> logger) : base(store, optionsAccessor, passwordHasher, userValidators,
//             passwordValidators, keyNormalizer, errors, services, logger)
//         {
//             _services = services;
//         }
//
//         public override Task<IdentityResult> CreateAsync(T user)
//         {
//             if (Options.Stores.ProtectPersonalData)
//             {
//                 base.FindByEmailAsync()
//             }
//
//             return base.CreateAsync(user);
//         }
//
//         public override Task<T> FindByEmailAsync(string email)
//         {
//             var user = base.FindByEmailAsync(email);
//         }
//
//         public override Task<T> FindByIdAsync(string userId)
//         {
//             return base.FindByIdAsync(userId);
//         }
//
//         public override Task<T> FindByNameAsync(string userName)
//         {
//             return base.FindByNameAsync(userName);
//         }
//
//         public override Task<T> FindByLoginAsync(string loginProvider, string providerKey)
//         {
//             return base.FindByLoginAsync(loginProvider, providerKey);
//         }
//
//         public override Task<IList<T>> GetUsersForClaimAsync(Claim claim)
//         {
//             return base.GetUsersForClaimAsync(claim);
//         }
//
//         public override Task<T> GetUserAsync(ClaimsPrincipal principal)
//         {
//             return base.GetUserAsync(principal);
//         }
//
//         public override Task<IList<T>> GetUsersInRoleAsync(string roleName)
//         {
//             return base.GetUsersInRoleAsync(roleName);
//         }
//
//         private string UnProtectPersonalData(string data)
//         {
//             var keyRing = _services.GetService<ILookupProtectorKeyRing>();
//             var protector = _services.GetService<ILookupProtector>();
//             
//             if (keyRing != null && protector != null)
//             {
//                 foreach (var key in keyRing.GetAllKeyIds())
//                 {
//                     var unprotectedData = protector.Unprotect(key, data);
//
//                     if (data != null)
//                         return unprotectedData;
//                 }
//             }
//         }
//
//         private string ProtectPersonalData(string data)
//         {
//             if (Options.Stores.ProtectPersonalData)
//             {
//                 var keyRing = _services.GetService<ILookupProtectorKeyRing>();
//                 var protector = _services.GetService<ILookupProtector>();
//                 return protector.Protect(keyRing.CurrentKeyId, data);
//             }
//
//             return data;
//         }
//     }
// }