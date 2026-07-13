namespace CAFRI.Infrastructure.Content;

public sealed class StoredContentImageViewModel
{
    public required string Url { get; init; }

    public required string FileName { get; init; }

    public required string Section { get; init; }

    public required string AbsolutePath { get; init; }

    public long SizeBytes { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}
