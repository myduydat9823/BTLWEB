namespace BTLWEB.Services.Interfaces;

public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
}
