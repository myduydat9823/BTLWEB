using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;

namespace BTLWEB.Repositories;

public class LoginLogRepository : ILoginLogRepository
{
    private readonly AppDbContext _dbContext;

    public LoginLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(LoginLog loginLog)
    {
        await _dbContext.LoginLogs.AddAsync(loginLog);
        await _dbContext.SaveChangesAsync();
    }
}
