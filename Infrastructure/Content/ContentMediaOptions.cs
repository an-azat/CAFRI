namespace CAFRI.Infrastructure.Content;

public sealed class ContentMediaOptions
{
    public const string SectionName = "ContentMedia";

    public string Provider { get; init; } = "Local";
    public string? RootPath { get; init; }
    public string? BucketName { get; init; }
    public string? AccountId { get; init; }
    public string? AccessKeyId { get; init; }
    public string? SecretAccessKey { get; init; }
    public string? ServiceUrl { get; init; }
    public string? PublicBaseUrl { get; init; }
}
