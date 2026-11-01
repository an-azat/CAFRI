using CAFRI.Application.Abstractions.Services;
using CAFRI.Infrastructure.Content;
using CAFRI.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class PublicationsController : Controller
{
    private readonly IPublicationContentService _publicationContentService;
    private readonly IProfessionalAccessService _professionalAccessService;
    private readonly ContentMediaStorageService _mediaStorage;

    public PublicationsController(
        IPublicationContentService publicationContentService,
        IProfessionalAccessService professionalAccessService,
        ContentMediaStorageService mediaStorage)
    {
        _publicationContentService = publicationContentService;
        _professionalAccessService = professionalAccessService;
        _mediaStorage = mediaStorage;
    }

    [HttpGet("/publications")]
    public async Task<IActionResult> Index(
        string? query,
        string? type,
        string? country,
        string? topic,
        string? sortBy,
        int page,
        CancellationToken cancellationToken) =>
        View(await _publicationContentService.GetIndexPageAsync(query, type, country, topic, sortBy, page == 0 ? 1 : page, cancellationToken));

    [HttpGet("/publications/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var detail = await _publicationContentService.GetDetailsAsync(slug, cancellationToken);
        if (detail is null)
        {
            return Redirect("/publications");
        }

        detail.CanAccessFullContent = !detail.RequiresSignIn ||
            await _professionalAccessService.CanReadProtectedContentAsync(User, cancellationToken);
        return View(detail);
    }

    // Documents are streamed through here so the access rules of the publication apply;
    // their storage URLs are not served as public static files.
    [HttpGet("/publications/{slug}/download")]
    public async Task<IActionResult> Download(string slug, string? file, CancellationToken cancellationToken)
    {
        var detail = await _publicationContentService.GetDetailsAsync(slug, cancellationToken);
        var documentUrl = detail?.Documents
            .Select(x => DocumentLink.Url(x.DownloadUrl))
            .FirstOrDefault(url => url != "#" && string.Equals(url, file, StringComparison.Ordinal));
        if (detail is null || documentUrl is null)
        {
            return NotFound();
        }

        if (detail.RequiresSignIn && !await _professionalAccessService.CanReadProtectedContentAsync(User, cancellationToken))
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
