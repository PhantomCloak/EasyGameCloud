using System;
using Autofac;
using Dapper.FluentMap;
using Identity.Plugin.Models;
using Identity.Plugin.Models.Mappers;
using Identity.Plugin.Repositories;
using Identity.Plugin.Repositories.ProtectedRepositories;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Identity.Plugin.Integration
{
    public class ProtectedRepositoryTests
    {
        private static IContainer _container;
        
        [ClassInitialize]
        private static void Init()
        {
            var builder = new ContainerBuilder();
            var keyRing = new IdentityDataProtectorKeyRing();
            var protector = new CustomPersonalDataProtector(keyRing);
            var userRepository = new IdentityUserRepository(null);
            var protectedUserRepository = new IdentityProtectedUserRepository(userRepository,protector);

            builder.RegisterInstance(protectedUserRepository).As<IIdentityUserRepository<ApplicationUser>>();
            
            _container = builder.Build();

            FluentMapper.Initialize(config =>
            {
                config.AddMap(new ApplicationUserMap());
                config.AddMap(new UserRoleMap());
                config.AddMap(new UserClaimMap());
            });
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
    }
}