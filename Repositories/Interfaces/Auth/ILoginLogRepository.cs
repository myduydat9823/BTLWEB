using BTLWEB.Models;

namespace BTLWEB.Repositories.Interfaces;

public interface ILoginLogRepository
{
    Task AddAsync(LoginLog loginLog);
}
