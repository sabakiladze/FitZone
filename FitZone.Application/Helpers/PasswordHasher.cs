using FitZone.Application.ServiceInterfaces;
using BCrypt.Net;



namespace FitZone.Infrastructure
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);

        }

        public bool Verify(string password, string passwordHash)
        {
           return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
