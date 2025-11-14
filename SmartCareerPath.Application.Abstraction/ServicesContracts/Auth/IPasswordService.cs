namespace SmartCareerPath.Application.Abstraction.ServicesContracts.Auth
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash, string passwordSalt = null);
        string GenerateSalt();
        bool IsStrongPassword(string password);
        (string Hash, string Salt) HashPasswordWithSalt(string password);

    }
}
