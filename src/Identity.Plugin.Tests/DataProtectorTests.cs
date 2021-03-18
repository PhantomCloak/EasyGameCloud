using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;

namespace Identity.Plugin.Tests
{
    public class CryptoTests
    {
        [Test]
        public void Encrypt_Decrypt_Same_Text_Are_Equal()
        {
            var keyRing = new IdentityDataProtectorKeyRing();
            var protector = new CustomPersonalDataProtector(keyRing);

            var plainText = "micheal@mail.com";
            
            var encryptedText = protector.Protect(plainText);
            var decryptedText = protector.Unprotect(encryptedText);
            
            Assert.AreEqual(plainText,decryptedText);
        }
    }
}