using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Identity.Plugin.Models;
using Identity.Plugin.Repositories;
using Identity.Plugin.Stores;
using Identity.Plugin.Tests.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Identity.Plugin.Tests
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
            var userManager = (UserManager<ApplicationUser>) _provider.GetRequiredService(typeof(UserManager<ApplicationUser>));

            var result = userManager.CreateAsync(new ApplicationUser("Bob", "bob@mail.com")).Result;

            if (!result.Succeeded)
            {
                Assert.Fail(result.Errors.ToString());
            }

            var userFromMail = userManager.FindByEmailAsync("bob@mail.com").Result;

            if (userFromMail == null)
            {
                Assert.Fail("User from mail has failed.");
            }

            var userFromUsername = userManager.FindByNameAsync("Bob").Result;

            if (userFromUsername == null)
            {
                Assert.Fail("User from name has failed.");
            }

            var userFromId = userManager.FindByIdAsync(userFromMail.Id).Result;

            if (userFromId == null)
            {
                Assert.Fail("User from Id has failed.");
            }

            Assert.Pass();
        }

        [TearDown]
        public void Cleanup()
        {
            var userRepository =
                (IdentityUserRepository) _provider.GetRequiredService(typeof(IIdentityUserRepository<ApplicationUser>));

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
            _services.AddScoped<IIdentityRoleRepository<IdentityRole>, IdentityRoleRepository>();
            _services.AddScoped<IPasswordHasher<ApplicationUser>, CustomPasswordHasher>();
            _services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            _services.AddScoped<IUserStore<ApplicationUser>, CustomUserStore<ApplicationUser>>();
            _services.AddScoped<ILookupProtectorKeyRing, IdentityDataProtectorKeyRing>();
            _services.AddScoped<ILookupProtector, IdentityLookupProtector>();
            _services.AddScoped<IPersonalDataProtector, CustomPersonalDataProtector>();
            _services.AddScoped<UserManager<ApplicationUser>>();

            _provider = _services.BuildServiceProvider();
        }
    }
}