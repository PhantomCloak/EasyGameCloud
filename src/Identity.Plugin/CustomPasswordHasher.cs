using System;
using System.Text;
using Identity.Plugin.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Identity.Plugin
{
    public class CustomPasswordHasher : IPasswordHasher<ApplicationUser>
    {
        private readonly PasswordHasherOptions _options;

        public CustomPasswordHasher(IOptions<PasswordHasherOptions> options)
        {
            _options = options?.Value ?? new PasswordHasherOptions();
        }

        public string HashPassword(ApplicationUser user, string password)
        {
            var saltBytes = new byte[128];
            var buffer = Encoding.Unicode.GetBytes(user.Email);

            var defaultBytesNeeded = 128 / 8;
            var bytesNeeded =  buffer.Length < defaultBytesNeeded ? buffer.Length : defaultBytesNeeded;
            
            Buffer.BlockCopy(buffer, 0, buffer, 0, bytesNeeded); //Trim to fit salt
            Buffer.BlockCopy(buffer, 0, saltBytes, 0, bytesNeeded);

            var hashedBuffer = HashPassword(password, saltBytes, KeyDerivationPrf.HMACSHA256, _options.IterationCount,
                256 / 8);

            return Convert.ToBase64String(hashedBuffer);
        }

        public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword,string providedPassword)
        {
            var provPass = HashPassword(user, providedPassword);
            if (provPass != hashedPassword)
            {
                return PasswordVerificationResult.Failed;
            }

            return PasswordVerificationResult.Success;
        }

        private static byte[] HashPassword(string password, byte[] salt, KeyDerivationPrf prf, int iterCount,int numBytesRequested)
        {
            return KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);
        }
    }
}