using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Hood.Services
{
    public class PasswordHasher
    {
        public byte[] Salt { get; set; }
        public string Base64Salt { get; set; }
        public string HashedPassword { get; set; }

        public PasswordHasher()
        {
            GenerateSalt();
        }
        public PasswordHasher(string base64Salt)
        {
            DecodeSalt(base64Salt);
        }

        private void GenerateSalt()
        {
            Salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(Salt);
            }
            Base64Salt = Convert.ToBase64String(Salt);
        }
        private void DecodeSalt(string base64Salt)
        {
            Base64Salt = base64Salt;
            Salt = Convert.FromBase64String(base64Salt);
        }

        public PasswordHasher HashPasswordWithSalt(string password)
        {
            HashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return this;
        }
    }
}