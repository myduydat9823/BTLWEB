using BTLWEB.Models;
using Microsoft.AspNetCore.Identity;

namespace BTLWEB.Services.Interfaces;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    PasswordVerificationResult VerifyPassword(User user, string hashedPassword, string password);
    string GenerateResetToken();
    string HashToken(string token);
}
