using System;
using System.Collections.Generic;
using Identity.Plugin.Models;
using Identity.Plugin.Repositories;
using Identity.Plugin.Repositories.ProtectedRepositories;
using Identity.Plugin.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Serilog;
using Assert = NUnit.Framework.Assert;
using ILogger = Serilog.ILogger;

namespace Identity.Plugin.Integration
{
    public class UserManagerTest
    {
        private ServiceProvider _provider;
        private readonly ServiceCollection _services = new ServiceCollection();

        [SetUp]
        public void Setup()
        {
            InitServices();
        }

        [Test]
        public void Create_User_Get_Same_User()
        {
            var userName = "Bob";
            var email = "bob@mail.com";

            var userManager =
                (CustomUserManager<ApplicationUser>) _provider.GetRequiredService(
                    typeof(CustomUserManager<ApplicationUser>));

            var result = userManager.CreateAsync(new ApplicationUser(userName, email)).Result;

            if (!result.Succeeded)
            {
                Assert.Fail(result.Errors.ToString());
            }

            var userFromMail = userManager.FindByEmailAsync("bob@mail.com").Result;

            if (userFromMail == null)
            {
                Assert.Fail("User from mail has failed.");
            }

            if (userFromMail.UserName != userName || userFromMail.Email != email)
            {
                Assert.Fail("Username or email is corrupted.");
            }
            
            var userFromUsername = userManager.FindByNameAsync("Bob").Result;

            if (userFromUsername == null)
            {
                Assert.Fail("User from name has failed.");
            }

            if (userFromUsername.UserName != userName || userFromUsername.Email != email)
            {
                Assert.Fail("Username or email is corrupted.");
            }
            
            var userFromId = userManager.FindByIdAsync(userFromMail.Id).Result;

            if (userFromId.UserName != userName || userFromId.Email != email)
            {
                Assert.Fail("Username or email is corrupted.");
            }
            
            if (userFromId == null)
            {
                Assert.Fail("User from Id has failed.");
            }

            Assert.Pass();
        }

        [TearDown]
        public void Cleanup()
        {
            var userRepository = (IIdentityUserRepository<ApplicationUser>)_provider.GetRequiredService(typeof(IIdentityUserRepository<ApplicationUser>));
            
            var users = userRepository.GetUsersAsync().Result;
            
            foreach (var user in users)
            {
                userRepository.DeleteUserAsync(user.Id);
            }
        }

        private void InitServices()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {
                    "Database:ConnectionString",
                    "Server=localhost;Port=5432;Database=identity;User Id=postgres;Password=badf00d11"
                }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            _services.AddSingleton<ILogger>(log);
            
            _services.AddSingleton<IConfiguration>(configuration);
            _services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });

            _services.Configure<PasswordHasherOptions>(options =>
            {
                // Default Lockout settings.
                options.IterationCount = 10000;
            });

            _services.AddScoped<IdentityErrorDescriber>();
            _services.AddSingleton(new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
            
            _services.AddScoped<IIdentityUserRepository<ApplicationUser>, IdentityUserRepository>();
            _services.Decorate<IIdentityUserRepository<ApplicationUser>, IdentityProtectedUserRepository>();
            
            _services.AddScoped<IIdentityRoleRepository<IdentityRole>, IdentityRoleRepository>();
            _services.AddScoped<IPasswordHasher<ApplicationUser>, CustomPasswordHasher>();
            _services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            _services.AddScoped<IUserStore<ApplicationUser>, CustomUserStore<ApplicationUser>>();
            _services.AddScoped<ILookupProtectorKeyRing, IdentityDataProtectorKeyRing>();
            _services.AddScoped<ILookupProtector, IdentityLookupProtector>();
            _services.AddScoped<IPersonalDataProtector, CustomPersonalDataProtector>();
            _services.AddScoped<Hasher>();
            _services.AddScoped<CustomUserManager<ApplicationUser>>();

            _provider = _services.BuildServiceProvider();
        }
    }
}