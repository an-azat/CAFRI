using System.Text.RegularExpressions;
using CAFRI.ViewModels.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CAFRI.Infrastructure.Content;

public sealed class ContentMediaStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
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

    private readonly IWebHostEnvironment _environment;

    public ContentMediaStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveImageAsync(IFormFile? file, string section, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        if (!IsSupportedImage(file))
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName);

        var rootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var folderPath = Path.Combine(rootPath, "uploads", "content", section);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/content/{section}/{fileName}";
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
        if (file is null || file.Length == 0)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrWhiteSpace(extension) && AllowedExtensions.Contains(extension);
    }

    public string GetAllowedExtensionsLabel() =>
        string.Join(", ", AllowedExtensions.OrderBy(x => x));

    public void DeleteImage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !url.StartsWith("/uploads/content/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var rootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var fullPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(rootPath, "uploads", "content"));
        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public IReadOnlyList<StoredContentImageViewModel> GetStoredImages()
    {
        var rootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
        var uploadsRoot = Path.Combine(rootPath, "uploads", "content");
        if (!Directory.Exists(uploadsRoot))
        {
            return [];
        }

        return Directory.GetFiles(uploadsRoot, "*.*", SearchOption.AllDirectories)
            .Where(filePath => AllowedExtensions.Contains(Path.GetExtension(filePath)))
            .Select(filePath =>
            {
                var fileInfo = new FileInfo(filePath);
                var relativePath = Path.GetRelativePath(rootPath, filePath).Replace('\\', '/');
                var relativeParts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var section = relativeParts.Length >= 3 ? relativeParts[2] : "other";

                return new StoredContentImageViewModel
                {
                    Url = "/" + relativePath,
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
}
