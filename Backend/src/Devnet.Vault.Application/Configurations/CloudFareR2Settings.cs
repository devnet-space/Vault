namespace Devnet.Vault.Application.Configurations;

public sealed class CloudFareR2Settings
{
    public string AccountId { get; set; } = string.Empty;

    // R2 Access Key
    public string AccessKeyId { get; set; } = string.Empty;

    // R2 Secret Key
    public string SecretAccessKey { get; set; } = string.Empty;

    // Bucket Name
    public string BucketName { get; set; } = string.Empty;

    // Optional: public domain/custom domain
    public string PublicBaseUrl { get; set; } = string.Empty;

    // R2 Endpoint
    public string ServiceUrl =>
        $"https://{AccountId}.r2.cloudflarestorage.com";

}
