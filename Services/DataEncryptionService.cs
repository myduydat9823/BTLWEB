using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using BTLWEB.Services.Interfaces;

namespace BTLWEB.Services;

public class DataEncryptionService : IDataEncryptionService
{
    private readonly IDataProtector _protector;

    public DataEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("BTLWEB.UserProfile.v1");
    }

    public string? Encrypt(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : _protector.Protect(value.Trim());
    }

    public string? Decrypt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return _protector.Unprotect(value);
        }
        catch
        {
            return null;
        }
    }

    public string? EncryptDate(DateTime? value)
    {
        return value is null ? null : _protector.Protect(value.Value.ToString("O", CultureInfo.InvariantCulture));
    }

    public DateTime? DecryptDate(string? value)
    {
        var decrypted = Decrypt(value);
        return DateTime.TryParse(decrypted, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date)
            ? date
            : null;
    }
}
