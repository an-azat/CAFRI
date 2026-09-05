using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using CAFRI.ViewModels.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CAFRI.Infrastructure.Content;

public sealed class ContentMediaStorageService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".jfif",
        ".png",
        ".bmp",
        ".webp",
        ".gif",
        ".avif"
    };

    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".csv",
        ".txt"
    };

    // Server-controlled Content-Type, keyed by extension — never trust the client-supplied
    // IFormFile.ContentType header for the value stored against the object in R2/S3.
    private static readonly Dictionary<string, string> ContentTypeByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".jfif"] = "image/jpeg",
        [".png"] = "image/png",
        [".bmp"] = "image/bmp",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".avif"] = "image/avif",
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".ppt"] = "application/vnd.ms-powerpoint",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".csv"] = "text/csv",
        [".txt"] = "text/plain"
    };

    private const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB
    private const long MaxDocumentSizeBytes = 25 * 1024 * 1024; // 25 MB

    private readonly IWebHostEnvironment _environment;
    private readonly ContentMediaOptions _options;
    private readonly Lazy<IAmazonS3?> _s3Client;

    public ContentMediaStorageService(IWebHostEnvironment environment, IOptions<ContentMediaOptions> options)
    {
        _environment = environment;
        _options = options.Value;
        _s3Client = new Lazy<IAmazonS3?>(CreateS3Client);
    }

    public bool UsesRemoteStorage =>
        string.Equals(_options.Provider, "R2", StringComparison.OrdinalIgnoreCase);

    public async Task<string?> SaveImageAsync(IFormFile? file, string section, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0 || !IsSupportedImage(file) || !await HasValidImageSignatureAsync(file, cancellationToken))
        {
            return null;
        }

        return await SaveFileAsync(file, section, "images", cancellationToken);
    }

    public async Task<IReadOnlyList<ContentImageItemViewModel>> SaveImagesAsync(
        IEnumerable<IFormFile>? files,
        string section,
        CancellationToken cancellationToken = default)
    {
        if (files is null)
        {
            return [];
        }

        var items = new List<ContentImageItemViewModel>();
        foreach (var file in files)
        {
            var url = await SaveImageAsync(file, section, cancellationToken);
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var baseName = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName), "[-_]+", " ").Trim();
            items.Add(new ContentImageItemViewModel
            {
                ImageUrl = url,
                Alt = baseName,
                Title = baseName,
                Caption = string.Empty
            });
        }

        return items;
    }

    public bool IsSupportedImage(IFormFile? file)
    {
        if (file is null || file.Length == 0 || file.Length > MaxImageSizeBytes)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrWhiteSpace(extension) && AllowedImageExtensions.Contains(extension);
    }

    public bool IsSupportedDocument(IFormFile? file)
    {
        if (file is null || file.Length == 0 || file.Length > MaxDocumentSizeBytes)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrWhiteSpace(extension) && AllowedDocumentExtensions.Contains(extension);
    }

    // Extension-based checks alone trust the filename; a renamed .html-as-.jpg would pass them.
    // Sniff the first bytes against each format's known magic number before persisting the file.
    private static async Task<bool> HasValidImageSignatureAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        var buffer = new byte[16];

        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
        stream.Position = 0;

        if (bytesRead < 4)
        {
            return false;
        }

        return extension.ToLowerInvariant() switch
        {
            ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
            ".jpg" or ".jpeg" or ".jfif" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
            ".gif" => buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38,
            ".bmp" => buffer[0] == 0x42 && buffer[1] == 0x4D,
            ".webp" => bytesRead >= 12 &&
                buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50,
            ".avif" => bytesRead >= 8 && buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70,
            _ => false
        };
    }

    public async Task<string?> SaveDocumentAsync(IFormFile? file, string section, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0 || !IsSupportedDocument(file))
        {
            return null;
        }

        return await SaveFileAsync(file, section, "documents", cancellationToken);
    }

    public string GetAllowedExtensionsLabel() =>
        string.Join(", ", AllowedImageExtensions.OrderBy(x => x));

    public string GetAllowedDocumentExtensionsLabel() =>
        string.Join(", ", AllowedDocumentExtensions.OrderBy(x => x));

    public Task DeleteImageAsync(string? url, CancellationToken cancellationToken = default) =>
        DeleteStoredFileAsync(url, AllowedImageExtensions, cancellationToken);

    public Task DeleteDocumentAsync(string? url, CancellationToken cancellationToken = default) =>
        DeleteStoredFileAsync(url, AllowedDocumentExtensions, cancellationToken);

    public string GetMediaRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_options.RootPath))
        {
            return Path.IsPathRooted(_options.RootPath)
                ? Path.GetFullPath(_options.RootPath)
                : Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.RootPath));
        }

        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        return Path.GetFullPath(Path.Combine(webRootPath, "uploads", "content"));
    }

    public Task<IReadOnlyList<StoredContentImageViewModel>> GetStoredImagesAsync(CancellationToken cancellationToken = default)
    {
        return UsesRemoteStorage
            ? GetStoredImagesFromRemoteAsync(cancellationToken)
            : Task.FromResult(GetStoredImagesFromLocal());
    }

    private async Task DeleteStoredFileAsync(string? url, IReadOnlySet<string> allowedExtensions, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (UsesRemoteStorage && IsRemoteUrl(url))
        {
            await DeleteRemoteObjectAsync(url, cancellationToken);
            return;
        }

        if (!url.StartsWith("/uploads/content/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = url["/uploads/content/".Length..].Replace('/', Path.DirectorySeparatorChar);
        var uploadsRoot = GetMediaRootPath();
        var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, relativePath));
        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            var extension = Path.GetExtension(fullPath);
            if (!string.IsNullOrWhiteSpace(extension) && !allowedExtensions.Contains(extension))
            {
                return;
            }

            File.Delete(fullPath);
        }
    }

    private IReadOnlyList<StoredContentImageViewModel> GetStoredImagesFromLocal()
    {
        var uploadsRoot = GetMediaRootPath();
        if (!Directory.Exists(uploadsRoot))
        {
            return [];
        }

        return Directory.GetFiles(uploadsRoot, "*.*", SearchOption.AllDirectories)
            .Where(filePath => AllowedImageExtensions.Contains(Path.GetExtension(filePath)))
            .Select(filePath =>
            {
                var fileInfo = new FileInfo(filePath);
                var relativePath = Path.GetRelativePath(uploadsRoot, filePath).Replace('\\', '/');
                var relativeParts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var section = relativeParts.Length >= 2 ? relativeParts[0] : "other";

                return new StoredContentImageViewModel
                {
                    Url = "/uploads/content/" + relativePath,
                    FileName = fileInfo.Name,
                    Section = section,
                    AbsolutePath = filePath,
                    SizeBytes = fileInfo.Length,
                    UpdatedAtUtc = new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero)
                };
            })
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenBy(item => item.FileName)
            .ToList();
    }

    private async Task<IReadOnlyList<StoredContentImageViewModel>> GetStoredImagesFromRemoteAsync(CancellationToken cancellationToken)
    {
        var client = _s3Client.Value;
        var bucketName = _options.BucketName;
        if (client is null || string.IsNullOrWhiteSpace(bucketName))
        {
            return [];
        }

        var items = new List<StoredContentImageViewModel>();
        string? continuationToken = null;

        do
        {
            var response = await client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucketName,
                ContinuationToken = continuationToken
            }, cancellationToken);

            foreach (var item in response.S3Objects.Where(x => AllowedImageExtensions.Contains(Path.GetExtension(x.Key))))
            {
                var section = item.Key.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "other";
                items.Add(new StoredContentImageViewModel
                {
                    Url = BuildRemotePublicUrl(item.Key),
                    FileName = Path.GetFileName(item.Key),
                    Section = section,
                    AbsolutePath = item.Key,
                    SizeBytes = item.Size,
                    UpdatedAtUtc = item.LastModified
                });
            }

            continuationToken = response.IsTruncated ? response.NextContinuationToken : null;
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return items
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenBy(item => item.FileName)
            .ToList();
    }

    private async Task<string?> SaveFileAsync(
        IFormFile file,
        string section,
        string contentTypeFolder,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";

        if (UsesRemoteStorage)
        {
            return await SaveFileToRemoteAsync(file, $"{section}/{contentTypeFolder}/{fileName}", cancellationToken);
        }

        var mediaRoot = GetMediaRootPath();
        var folderPath = Path.Combine(mediaRoot, section, contentTypeFolder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/content/{section}/{contentTypeFolder}/{fileName}";
    }

    private async Task<string?> SaveFileToRemoteAsync(
        IFormFile file,
        string objectKey,
        CancellationToken cancellationToken)
    {
        var client = _s3Client.Value;
        var bucketName = _options.BucketName;
        if (client is null || string.IsNullOrWhiteSpace(bucketName))
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName);
        var contentType = extension is not null && ContentTypeByExtension.TryGetValue(extension, out var mapped)
            ? mapped
            : "application/octet-stream";

        await using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = stream,
            AutoCloseStream = false,
            ContentType = contentType,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        };

        await client.PutObjectAsync(request, cancellationToken);
        return BuildRemotePublicUrl(objectKey);
    }

    private async Task DeleteRemoteObjectAsync(string url, CancellationToken cancellationToken)
    {
        var client = _s3Client.Value;
        var bucketName = _options.BucketName;
        var objectKey = GetRemoteObjectKey(url);
        if (client is null || string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(objectKey))
        {
            return;
        }

        await client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        }, cancellationToken);
    }

    private bool IsRemoteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return false;
        }

        return url.StartsWith(_options.PublicBaseUrl.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase);
    }

    private string? GetRemoteObjectKey(string url)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return null;
        }

        var baseUrl = _options.PublicBaseUrl.TrimEnd('/') + "/";
        if (!url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Uri.UnescapeDataString(url[baseUrl.Length..]);
    }

    private string BuildRemotePublicUrl(string objectKey)
    {
        var baseUrl = (_options.PublicBaseUrl ?? string.Empty).TrimEnd('/');
        return $"{baseUrl}/{Uri.EscapeDataString(objectKey).Replace("%2F", "/")}";
    }

    private IAmazonS3? CreateS3Client()
    {
        if (!UsesRemoteStorage ||
            string.IsNullOrWhiteSpace(_options.AccountId) ||
            string.IsNullOrWhiteSpace(_options.AccessKeyId) ||
            string.IsNullOrWhiteSpace(_options.SecretAccessKey))
        {
            return null;
        }

        var serviceUrl = !string.IsNullOrWhiteSpace(_options.ServiceUrl)
            ? _options.ServiceUrl
            : $"https://{_options.AccountId}.r2.cloudflarestorage.com";

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        };

        return new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
    }
}
