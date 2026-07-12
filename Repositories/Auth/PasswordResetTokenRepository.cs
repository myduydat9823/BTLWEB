using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _dbContext;

    public PasswordResetTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _dbContext.PasswordResetTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
    }

    public Task<PasswordResetToken?> GetValidTokenAsync(string tokenHash, string email)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();

        return _dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash
                && x.UsedAtUtc == null
                && x.ExpiresAtUtc > DateTime.UtcNow
                && x.User != null
                && x.User.NormalizedEmail == normalizedEmail);
    }

    public async Task MarkUsedAsync(PasswordResetToken token)
    {
        token.UsedAtUtc = DateTime.UtcNow;
        _dbContext.PasswordResetTokens.Update(token);
        await _dbContext.SaveChangesAsync();
    }
}
