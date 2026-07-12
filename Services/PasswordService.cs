using System.Security.Cryptography;
using System.Text;
using BTLWEB.Models;
using BTLWEB.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BTLWEB.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher;

    public PasswordService(PasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyPassword(User user, string hashedPassword, string password)
    {
        return _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
    }

    public string GenerateResetToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
