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
    public IActionResult Index(
        string? query,
        string? type,
        string? country,
        string? topic,
        string? tab,
        int page = 1) =>
        View(_publicationContentService.GetIndexPage(query, type, country, topic, tab, page));

    [HttpGet("/publications/{slug}")]
    public IActionResult Details(string slug)
    {
        var detail = _publicationContentService.GetDetails(slug);
        if (detail is null)
        {
            return Redirect("/publications");
        }

        detail.CanAccessFullContent = User.Identity?.IsAuthenticated == true;
        return View(detail);
    }
}
