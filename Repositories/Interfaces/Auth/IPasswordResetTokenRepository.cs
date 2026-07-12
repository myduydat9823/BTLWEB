using BTLWEB.Models;

namespace BTLWEB.Repositories.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetValidTokenAsync(string tokenHash, string email);
    Task MarkUsedAsync(PasswordResetToken token);
}
