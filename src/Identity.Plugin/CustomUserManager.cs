using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Plugin
{
    public class CustomUserManager<T> : UserManager<T> where T : class
    {
        private readonly IUserStore<T> _store;
        private readonly Hasher _hasher;

        public CustomUserManager(IUserStore<T> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<T> passwordHasher,
            IEnumerable<IUserValidator<T>> userValidators,
            IEnumerable<IPasswordValidator<T>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<T>> logger,
            Hasher hasher) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
            keyNormalizer, errors, services, logger)
        {
            _store = store;
            _hasher = hasher;
        }

        public override async Task UpdateNormalizedEmailAsync(T user)
        {
            if (_store is IUserEmailStore<T> store)
            {
                var email = await GetEmailAsync(user);
                var normalizedEmail = NormalizeEmail(email);
                await store.SetNormalizedEmailAsync(user, _hasher.Hash(normalizedEmail), CancellationToken);
            }
        }

        public override async Task UpdateNormalizedUserNameAsync(T user)
        {
            var username = await GetUserNameAsync(user);
            var normalizedUsername = NormalizeName(username);
            await _store.SetNormalizedUserNameAsync(user, _hasher.Hash(normalizedUsername), CancellationToken);
        }

        public override async Task<T> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            var store = _store as IUserEmailStore<T>;
            
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }
            
            email = NormalizeEmail(email);

            T user = null;
            foreach (var algorithm in _hasher.AvailableAlgorithms())
            {
                var hashedEmail = _hasher.Hash(email,algorithm);
                user = await store.FindByEmailAsync(hashedEmail, CancellationToken);
                
                if(user != null)
                    break;
            }
            

            return user;
        }

        public override async Task<T> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            var store = _store as IUserEmailStore<T>;
            
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }
            
            userName = NormalizeName(userName);

            T user = null;
            foreach (var algorithm in _hasher.AvailableAlgorithms())
            {
                var hashedEmail = _hasher.Hash(userName,algorithm);
                user = await store.FindByNameAsync(hashedEmail, CancellationToken);
                
                if(user != null)
                    break;
            }
            

            return user;
        }
    }
}