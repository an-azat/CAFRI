using CAFRI.Application.Abstractions.Services;
using CAFRI.Infrastructure.Content;
using CAFRI.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class IntelligenceController : Controller
{
    private readonly IIntelligenceContentService _intelligenceContentService;
    private readonly IProfessionalAccessService _professionalAccessService;
    private readonly ContentMediaStorageService _mediaStorage;

    public IntelligenceController(
        IIntelligenceContentService intelligenceContentService,
        IProfessionalAccessService professionalAccessService,
        ContentMediaStorageService mediaStorage)
    {
        _intelligenceContentService = intelligenceContentService;
        _professionalAccessService = professionalAccessService;
        _mediaStorage = mediaStorage;
    }

    [HttpGet("/intelligence")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _intelligenceContentService.GetLandingPageAsync(cancellationToken));

    [HttpGet("/intelligence/archive")]
    public async Task<IActionResult> Archive(
        string? search,
        string? country,
        string? category,
        string? institution,
        string? dateRange,
        string? sortBy,
        int page,
        CancellationToken cancellationToken) =>
        View(await _intelligenceContentService.GetArchivePageAsync(search, country, category, institution, dateRange, sortBy, page == 0 ? 1 : page, cancellationToken));

    [HttpGet("/intelligence/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var item = await _intelligenceContentService.GetDetailsAsync(slug, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        item.CanAccessFullContent = await _professionalAccessService.CanReadProtectedContentAsync(User, cancellationToken);
        return View(item);
    }

    // Streams an attached document only to readers who can open the full article.
    [HttpGet("/intelligence/{slug}/download")]
    public async Task<IActionResult> Download(string slug, string? file, CancellationToken cancellationToken)
    {
        var item = await _intelligenceContentService.GetDetailsAsync(slug, cancellationToken);
        var documentUrl = item?.AvailableDownloads
            .Select(x => DocumentLink.Url(x.DownloadUrl))
            .FirstOrDefault(url => url != "#" && string.Equals(url, file, StringComparison.Ordinal));
        if (item is null || documentUrl is null)
        {
            return NotFound();
        }

        if (item.IsPremium && !await _professionalAccessService.CanReadProtectedContentAsync(User, cancellationToken))
        {
            return User.Identity?.IsAuthenticated == true
                ? Forbid()
                : Challenge();
        }

        var stored = await _mediaStorage.OpenDocumentAsync(documentUrl, cancellationToken);
        if (stored is not null)
        {
            return File(stored.Content, stored.ContentType, stored.FileName);
        }

        return Uri.TryCreate(documentUrl, UriKind.Absolute, out var external) &&
               (external.Scheme == Uri.UriSchemeHttp || external.Scheme == Uri.UriSchemeHttps)
            ? Redirect(documentUrl)
            : NotFound();
    }
}
