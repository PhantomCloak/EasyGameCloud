using System;
using System.Text;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin
{
    public class CustomPasswordHasher : IPasswordHasher<ApplicationUser>
    {
        private readonly PasswordHasherOptions _options;

        public CustomPasswordHasher(PasswordHasherOptions options)
        {
            _options = options;
        }

        public string HashPassword(ApplicationUser user, string password)
        {
            var saltBytes = new byte[128];
            
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(user.Email),0,saltBytes,0, 128 / 8);
            
            var hashedBuffer = HashPassword(password, saltBytes, KeyDerivationPrf.HMACSHA256,_options.IterationCount,256 / 8);

            return Encoding.UTF8.GetString(hashedBuffer);
        }

        public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
        {
            var provPass = HashPassword(user,providedPassword);
            if (provPass != hashedPassword)
            {
                return PasswordVerificationResult.Failed;
            }

            return PasswordVerificationResult.Success;
        }
        
        private static byte[] HashPassword(string password,byte[] salt, KeyDerivationPrf prf, int iterCount, int numBytesRequested)
        {
            return KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);
        }
    }
}