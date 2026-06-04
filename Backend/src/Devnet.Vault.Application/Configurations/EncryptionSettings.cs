namespace Devnet.Vault.Application.Configurations;

public sealed class EncryptionSettings
{
    public string AesKey { get; set; } = null!;
    public string AesIV { get; set; } = null!;
    public string AppKey { get; set; } = null!;
}
