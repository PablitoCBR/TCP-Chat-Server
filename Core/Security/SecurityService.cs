using Core.Security.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Core.Security
{
    public class SecurityService : ISecurityService
    {
        private SecuritySettings Settings { get; }

        public SecurityService(IOptions<SecuritySettings> options)
        {
            this.Settings = options.Value;
        }

        public (byte[] passwordHash, byte[] passwordSalt) GenerateHash(string password)
        {
            byte[] saltHash = this.GenerateSalt();
            byte[] passwordHash = this.GenerateHash(password, saltHash);
            return (passwordHash, saltHash);
        }

        public bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            byte[] givenPasswordHash = this.GenerateHash(password, passwordSalt);

            if (givenPasswordHash.Length != passwordHash.Length)
                return false;

            for (int i = 0; i < passwordHash.Length; i++)
                if (givenPasswordHash[i] != passwordHash[i])
                    return false;
            return true;
        }

        private byte[] GenerateSalt()
        {
            using (RNGCryptoServiceProvider saltGenerator = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[this.Settings.SaltByteSize];
                saltGenerator.GetBytes(salt);
                return salt;
            }
        }

        private byte[] GenerateHash(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes hashGenerator = new Rfc2898DeriveBytes(password, salt))
            {
                hashGenerator.IterationCount = this.Settings.HashingIterationsCount;
                return hashGenerator.GetBytes(this.Settings.HashByteSize);
            }
        }
    }
}
