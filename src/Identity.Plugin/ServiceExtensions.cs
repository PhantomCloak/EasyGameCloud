using Identity.Plugin.Models;
using Identity.Plugin.Repositories;
using Identity.Plugin.Repositories.ProtectedRepositories;
using Identity.Plugin.Stores;
using IdentityServer4;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Plugin
{
    public static class ServiceExtensions
    {
        public static void AddCustomIdentity(this IServiceCollection instance)
        {
            instance.AddScoped<IdentityErrorDescriber>();
            instance.AddScoped<IIdentityUserRepository<ApplicationUser>, IdentityUserRepository>();
            instance.Decorate<IIdentityUserRepository<ApplicationUser>, IdentityProtectedUserRepository>();
            instance.AddScoped<IIdentityRoleRepository<IdentityRole>, IdentityRoleRepository>();
            instance.AddScoped<IPasswordHasher<ApplicationUser>, CustomPasswordHasher>();
            instance.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            instance.AddScoped<IUserStore<ApplicationUser>, CustomUserStore<ApplicationUser>>();
            instance.AddScoped<ILookupProtectorKeyRing, IdentityDataProtectorKeyRing>();
            instance.AddScoped<ILookupProtector, IdentityLookupProtector>();
            instance.AddScoped<IPersonalDataProtector, CustomPersonalDataProtector>();
            instance.AddScoped<Hasher>();
            instance.AddScoped<CustomUserManager<ApplicationUser>>();
            instance.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                    options.EmitStaticAudienceClaim = true;
                })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ProfileService>();
        }

        public static void AddGoogle(this IServiceCollection instance,string clientId,string clientSecret)
        {
            instance.AddAuthentication().AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                });
        }
    }
}