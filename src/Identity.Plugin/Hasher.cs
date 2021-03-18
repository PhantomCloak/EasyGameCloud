using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Identity.Plugin
{
    public class Hasher
    {
        private readonly ILogger _logger;
        private byte[] _salt;

        public Hasher(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            string saltValue;

            try
            {
                saltValue = configuration["Security:Cryptography:DefaultHashSalt"];
            }
            catch (Exception e)
            {
                saltValue = string.Empty;
                _logger.Error("Default hash salt not included in app settings error msg: {0}.", e.Message);
            }

            if (string.IsNullOrEmpty(saltValue))
            {
                saltValue = string.Empty;
                _logger.Error("Default hash salt not found fallback to no-salt mode.");
            }

            _salt = Encoding.Unicode.GetBytes(saltValue);
        }

        public string Hash(string value, HashAlgorithm type = null)
        {
            type ??= AvailableAlgorithms().First();
            
            var buffer = GetPlainBuffer(value);

            var computeHash = type.ComputeHash(buffer);
            
            return Convert.ToBase64String(computeHash);
        }

        public List<HashAlgorithm> AvailableAlgorithms()
        {
            var algorithms = new List<HashAlgorithm> {new SHA256Managed()};
            return algorithms;
        }

        private byte[] GetPlainBuffer(string text)
        {
            var combinedBuffer = new byte[Encoding.Unicode.GetByteCount(text) + _salt.Length];
            
            var plainText = Encoding.Unicode.GetBytes(text);
            
            Buffer.BlockCopy(plainText,0,combinedBuffer,0,plainText.Length);
            Buffer.BlockCopy(_salt,0,combinedBuffer,plainText.Length,_salt.Length);

            return combinedBuffer;
        }
    }
}