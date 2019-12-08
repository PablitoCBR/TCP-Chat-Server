namespace Core.Services.Security.Interfaces
{
    public interface ISecurityService
    {
        (byte[] passwordHash, byte[] passwordSalt) GenerateHash(string password);

        bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}
