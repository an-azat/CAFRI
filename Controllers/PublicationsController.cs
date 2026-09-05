using CAFRI.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class PublicationsController : Controller
{
    private readonly IPublicationContentService _publicationContentService;

    public PublicationsController(IPublicationContentService publicationContentService)
    {
        _publicationContentService = publicationContentService;
    }

    [HttpGet("/publications")]
    public async Task<IActionResult> Index(
        string? query,
        string? type,
        string? country,
        string? topic,
        string? tab,
        int page,
        CancellationToken cancellationToken) =>
        View(await _publicationContentService.GetIndexPageAsync(query, type, country, topic, tab, page == 0 ? 1 : page, cancellationToken));

    [HttpGet("/publications/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var detail = await _publicationContentService.GetDetailsAsync(slug, cancellationToken);
        if (detail is null)
        {
            return Redirect("/publications");
        }

        detail.CanAccessFullContent = User.Identity?.IsAuthenticated == true;
        return View(detail);
    }
}
