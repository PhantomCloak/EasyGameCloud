using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace Identity.Plugin
{
    internal class CryptoBlob
    {
        public CryptoBlob()
        {
        }

        public CryptoBlob(string encodedPayload)
        {
            FromBase64(encodedPayload);
        }

        public string KeyId { get; set; }
        public byte[] InitializationVector { get; set; }
        public int SymmetricAlgorithmId { get; set; }
        public byte[] Signature { get; set; }
        public byte[] Payload { get; set; }

        public string ToBase64()
        {
            var manuelSerializer = new ManuelSerializer();

            manuelSerializer.Write(KeyId);
            manuelSerializer.Write(InitializationVector);
            manuelSerializer.Write(SymmetricAlgorithmId);
            manuelSerializer.Write(Signature);
            manuelSerializer.Write(Payload);

            return Convert.ToBase64String(manuelSerializer.GetBytes());
        }

        private void FromBase64(string encodedBlob)
        {
            var blobInBytes = Convert.FromBase64String(encodedBlob);

            var manuelSerializer = new ManuelSerializer(blobInBytes);

            KeyId = manuelSerializer.ReadString();
            InitializationVector = manuelSerializer.ReadBytes();
            SymmetricAlgorithmId = manuelSerializer.ReadInt();
            Signature = manuelSerializer.ReadBytes();
            Payload = manuelSerializer.ReadBytes();
        }
    }
    
    public class CustomPersonalDataProtector : IPersonalDataProtector
    {
        private readonly ProtectorAlgorithm _defaultAlgorithm = ProtectorAlgorithm.Aes256Hmac512;
        private readonly ILookupProtectorKeyRing _keyRing;


        public CustomPersonalDataProtector(ILookupProtectorKeyRing keyRing)
        {
            _keyRing = keyRing;
        }

        public string Protect(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }

            ProtectorAlgorithmHelper.GetAlgorithms(
                _defaultAlgorithm,
                out var encryptingAlgorithm,
                out var signingAlgorithm,
                out var keyDerivationIterationCount);

            var blob = new CryptoBlob
            {
                KeyId = _keyRing.CurrentKeyId,
                SymmetricAlgorithmId = (int) _defaultAlgorithm,
                InitializationVector = encryptingAlgorithm.IV
            };

            var masterKey = GetKey(blob.KeyId);
            var encryptionKey =
                GenerateEncryptionKey(masterKey, encryptingAlgorithm.KeySize, keyDerivationIterationCount);

            encryptingAlgorithm.Key = encryptionKey;

            var encryptedPayload = EncryptData(data, encryptingAlgorithm);

            blob.Payload = encryptedPayload;
            blob.Signature = GetPayloadSignature(
                encryptedPayload: encryptedPayload,
                iv: encryptingAlgorithm.IV,
                masterKey: masterKey,
                symmetricAlgorithmKeySize: encryptingAlgorithm.KeySize,
                hashAlgorithm: signingAlgorithm,
                keyDerivationIterationCount: keyDerivationIterationCount);

            encryptingAlgorithm.Clear();
            signingAlgorithm.Clear();
            encryptingAlgorithm.Dispose();
            signingAlgorithm.Dispose();

            return blob.ToBase64();
        }

        public string Unprotect(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
    
            var blob = new CryptoBlob(data);

            ProtectorAlgorithmHelper.GetAlgorithms(
                (ProtectorAlgorithm) blob.SymmetricAlgorithmId,
                out var encryptingAlgorithm,
                out var signingAlgorithm,
                out int keyDerivationIterationCount);

            var masterKey = GetKey(blob.KeyId);
            var decryptionKey =
                GenerateEncryptionKey(masterKey, encryptingAlgorithm.KeySize, keyDerivationIterationCount);

            encryptingAlgorithm.Key = decryptionKey;
            encryptingAlgorithm.IV = blob.InitializationVector;

            var signature = GetPayloadSignature(blob.Payload,
                blob.InitializationVector,
                masterKey,
                encryptingAlgorithm.KeySize,
                signingAlgorithm,
                keyDerivationIterationCount);

            if (!ByteArraysEqual(signature, blob.Signature))
            {
                throw new CryptographicException(@"Invalid Signature.");
            }

            var decryptedData = DecryptData(blob.Payload, encryptingAlgorithm);

            encryptingAlgorithm.Clear();
            encryptingAlgorithm.Dispose();

            return Encoding.UTF8.GetString(decryptedData);
        }

        private byte[] GetKey(string keyId)
        {
            return Convert.FromBase64String(_keyRing[keyId]);
        }

        private static byte[] EncryptData(string data, SymmetricAlgorithm encryptingAlgorithm)
        {
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptingAlgorithm.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(Encoding.UTF8.GetBytes(data));
            cs.FlushFinalBlock();

            return ms.ToArray();
        }

        private static byte[] DecryptData(byte[] data, SymmetricAlgorithm encryptingAlgorithm)
        {
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptingAlgorithm.CreateDecryptor(), CryptoStreamMode.Write);
            
            cs.Write(data);
            cs.FlushFinalBlock();

            return ms.ToArray();
        }
        
        private static byte[] GetPayloadSignature(
            byte[] encryptedPayload,
            byte[] iv,
            byte[] masterKey,
            int symmetricAlgorithmKeySize,
            KeyedHashAlgorithm hashAlgorithm,
            int keyDerivationIterationCount)
        {
            var payloadAndIvBuffer = new byte[encryptedPayload.Length + iv.Length];

            Buffer.BlockCopy(encryptedPayload, 0, payloadAndIvBuffer, 0, encryptedPayload.Length);
            Buffer.BlockCopy(iv, 0, payloadAndIvBuffer, encryptedPayload.Length, iv.Length);

            hashAlgorithm.Key = GenerateSigningKey(masterKey, symmetricAlgorithmKeySize, keyDerivationIterationCount);

            byte[] signature = hashAlgorithm.ComputeHash(payloadAndIvBuffer);

            hashAlgorithm.Clear();

            return signature;
        }

        private static byte[] GenerateSigningKey(byte[] keySalt, int symmetricAlgorithmKeySize, int keyIterationCount)
        {
            return KeyDerivation.Pbkdf2(@"PersonalDataSigning", keySalt, KeyDerivationPrf.HMACSHA512, keyIterationCount,
                symmetricAlgorithmKeySize / 8);
        }

        private static byte[] GenerateEncryptionKey(byte[] key, int algorithmKeySize, int keyDerivationIterationCount)
        {
            return KeyDerivation.Pbkdf2(
                @"PersonalDataEncryption",
                key,
                KeyDerivationPrf.HMACSHA512,
                keyDerivationIterationCount,
                algorithmKeySize / 8);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }

            return areSame;
        }
    }
}