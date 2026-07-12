namespace BTLWEB.Services.Interfaces;

public interface IDataEncryptionService
{
    string? Encrypt(string? value);
    string? Decrypt(string? value);
    string? EncryptDate(DateTime? value);
    DateTime? DecryptDate(string? value);
}
