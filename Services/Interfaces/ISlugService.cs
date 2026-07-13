namespace BTLWEB.Services.Interfaces;

public interface ISlugService
{
    string GenerateSlug(string title);
    Task<string> GenerateUniqueSlugAsync(string title, Func<string, Task<bool>> slugExistsAsync);
}
