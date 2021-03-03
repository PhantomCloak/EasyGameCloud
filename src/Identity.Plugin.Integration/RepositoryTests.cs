using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Autofac;
using Dapper.FluentMap;
using Identity.Plugin.Models;
using Identity.Plugin.Models.Mappers;
using Identity.Plugin.Repositories;
using IdentityServer.Data;
using IdentityServer.Data.DapperMapperProfiles;
using IdentityServer.Repositories;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdentityServer.Integration
{
    [TestClass]
    public class RepositoryTests
    {
        private static IContainer _container;
        private static IdentityRoleRepository wooRee = null;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var builder = new ContainerBuilder();
            
            //circular dependency
            var userRepository = new IdentityUserRepository();
            var roleRepository = new IdentityRoleRepository();

            wooRee = roleRepository;

            builder.RegisterInstance(userRepository).As<IIdentityUserRepository<ApplicationUser>>();
            builder.RegisterInstance(roleRepository).As<IIdentityRoleRepository<IdentityRole>>();
            
            
            _container = builder.Build();

            FluentMapper.Initialize(config =>
            {
                config.AddMap(new ApplicationUserMap());
                config.AddMap(new UserRoleMap());
                config.AddMap(new UserClaimMap());
            });
        }

        [TestMethod]
        public void Add_MultipleRole_GetAddedCount()
        {
            using var resolver = _container.BeginLifetimeScope();
            var roleRepository = resolver.Resolve<IIdentityRoleRepository<IdentityRole>>();

            int iteration = 5;

            for (int i = 0; i < iteration; i++)
            {
                var role = GetTestRole();
                role.Id = new Random().Next(0, 255).ToString();

                roleRepository.CreateRoleAsync(role, null).Wait();
            }

            var returnRoles = roleRepository.GetRolesAsync().Result;

            Assert.IsTrue(returnRoles.Count() == iteration);
        }

        [TestMethod]
        public void Add_SingleRole_ReturnSameRole()
        {
            using var resolver = _container.BeginLifetimeScope();
            var roleRepository = resolver.Resolve<IIdentityRoleRepository<IdentityRole>>();
            var compareLogic = new CompareLogic();
            var testRole = GetTestRole();
            
            compareLogic.Config.IgnoreProperty<IdentityRole>(x => x.Id);
            compareLogic.Config.IgnoreProperty<IdentityRole>(x => x.ConcurrencyStamp);

            roleRepository.CreateRoleAsync(testRole, null);
            var returnRole = roleRepository.GetRoleByNameAsync(testRole.Name).Result;

            var result = compareLogic.Compare(testRole, returnRole);

            Assert.IsTrue(result.AreEqual,
                $"Added and retrieved users aren't identical diffirences are {string.Join(",", result.Differences)}");
        }

        [TestMethod]
        public void Add_SingleUser_WithRole_ReturnsSameRoleUser()
        {
            //Arrange 
            using var resolver = _container.BeginLifetimeScope();

            var userRepository = resolver.Resolve<IIdentityUserRepository<ApplicationUser>>();
            var roleRepository = resolver.Resolve<IIdentityRoleRepository<IdentityRole>>();

            var compareLogic = new CompareLogic();

            compareLogic.Config.IgnoreProperty<IdentityRole>(x => x.Id);
            compareLogic.Config.IgnoreProperty<IdentityRole>(x => x.ConcurrencyStamp);

            var testUser = GetTestUser();
            var testRole = GetTestRole();

            roleRepository.CreateRoleAsync(testRole, null).Wait();
            userRepository.CreateUserAsync(testUser).Wait();
            userRepository.AddRoleToUserAsync(testUser, testRole);

            //Act

            var returnUser = userRepository.GetUserFromEmail(testUser.NormalizedEmail).Result;
            userRepository.AddRoleToUserAsync(returnUser, testRole);

            var returnRole = userRepository.GetUserRolesAsync(returnUser).Result.FirstOrDefault();

            var result = compareLogic.Compare(testRole, returnRole);

            //Assert
            Assert.IsTrue(result.AreEqual,
                $"Added and retrieved roles aren't identical diffirences are {string.Join(",", result.Differences)}");
        }

        [TestMethod]
        public void Add_RoleClaims_ReturnSameRoleClaims()
        {
            //Arrange 
            using var resolver = _container.BeginLifetimeScope();
            var roleRepository = resolver.Resolve<IIdentityRoleRepository<IdentityRole>>();
            var compareLogic = new CompareLogic();

            var testRole = GetTestRole();

            var roleClaims = new List<Claim>
            {
                new(ClaimTypes.Role, "Administrator"),
                new(ClaimTypes.Actor, "User"),
                new(ClaimTypes.Country, "UK")
            };

            roleRepository.CreateRoleAsync(testRole,roleClaims).Wait();

            var claims = roleRepository.GetRoleClaims("admin").Result;
            var result = compareLogic.Compare(roleClaims, claims);

            Assert.IsTrue(
                result.AreEqual,$"Added role claims users aren't identical diffirences are {string.Join(",", result.Differences)}");
        }

        [TestMethod]
        public void Add_SingleUser_ReturnsSameUser()
        {
            //Arrange 
            using var resolver = _container.BeginLifetimeScope();
            var userRepository = resolver.Resolve<IIdentityUserRepository<ApplicationUser>>();
            var compareLogic = new CompareLogic();
            compareLogic.Config.IgnoreProperty<ApplicationUser>(x => x.Id);
            var testUser = GetTestUser();

            //Act
            userRepository.CreateUserAsync(testUser).Wait();
            var returnUser = userRepository.GetUserFromEmail(testUser.NormalizedEmail).Result;
            var result = compareLogic.Compare(testUser, returnUser);

            //Assert
            Assert.IsTrue(result.AreEqual,
                $"Added and retrieved users aren't identical diffirences are {string.Join(",", result.Differences)}");
        }

        [TestMethod]
        public void Add_SingleUser_WithClaims_ReturnSameUserClaims()
        {
            //Arrange 
            using var resolver = _container.BeginLifetimeScope();
            var userRepository = resolver.Resolve<IIdentityUserRepository<ApplicationUser>>();
            var compareLogic = new CompareLogic();
            var testUser = GetTestUser();

            compareLogic.Config.MembersToInclude.Add("Type");
            compareLogic.Config.MembersToInclude.Add("Value");

            var claims = new Claim[]
            {
                new(ClaimTypes.Expiration, int.MaxValue.ToString()),
                new(ClaimTypes.Role, "Administrator")
            };
            
            //Act
            userRepository.CreateUserAsync(testUser).Wait();
            var user = userRepository.GetUserFromEmail(testUser.NormalizedEmail).Result;
            userRepository.AddClaimsToUser(user, claims).Wait();

            var retrivedClaims = userRepository.GetUserClaims(user).Result.ToArray();
            var result = compareLogic.Compare(retrivedClaims, claims);

            //Assert
            Assert.IsTrue(result.AreEqual,
                $"Added and retrieved users aren't identical diffirences are {string.Join(",", result.Differences)}");
        }
        

        private ApplicationUser GetTestUser()
        {
            return new()
            {
                Id = "1000",
                UserName = "Bob",
                PasswordHash = "1a901103602e70ae0f8556558de693629bf76511d74ac2f990df035f381f3bf6",
                NormalizedUserName = "BOB",
                NormalizedEmail = "BOB@HOTMAIL.COM",
                Email = "bob@hotmail.com",
                IsEmailVerified = true,
                PhoneNumber = "(555) 555-1234",
                IsPhoneNumberVerified = true,
                SecurityStamp = "289JKASJDOAS29DJCJKCJKJA",
                IsTwoFactorEnabled = false,
                AccessFailedCount = 0,
                IsLockoutEnabled = false,
                CreatedOn = DateTime.Today
            };
        }

        private IdentityRole GetTestRole()
        {
            return new()
            {
                Name = "admin",
                NormalizedName = "ADMIN"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var resolver = _container.BeginLifetimeScope();
            var userRepository = resolver.Resolve<IIdentityUserRepository<ApplicationUser>>();
            var roleRepository = resolver.Resolve<IIdentityRoleRepository<IdentityRole>>();

            var users = userRepository.GetUsersAsync().Result;

            foreach (var user in users)
            {
                userRepository.DeleteUserAsync(user.Id);
            }

            var roles = roleRepository.GetRolesAsync().Result;

            foreach (var role in roles)
            {
                roleRepository.DeleteRoleAsync(role.Id);
            }
        }
    }
}