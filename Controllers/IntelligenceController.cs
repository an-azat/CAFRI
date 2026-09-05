using CAFRI.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class IntelligenceController : Controller
{
    private readonly IIntelligenceContentService _intelligenceContentService;

    public IntelligenceController(IIntelligenceContentService intelligenceContentService)
    {
        _intelligenceContentService = intelligenceContentService;
    }

    [HttpGet("/intelligence")]
    public async Task<IActionResult> Index(
        string? search,
        string? country,
        string? category,
        string? institution,
        string? dateRange,
        string? sortBy,
        int page,
        CancellationToken cancellationToken) =>
        View(await _intelligenceContentService.GetIndexPageAsync(search, country, category, institution, dateRange, sortBy, page == 0 ? 1 : page, cancellationToken));

    [HttpGet("/intelligence/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var item = await _intelligenceContentService.GetDetailsAsync(slug, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        item.CanAccessFullContent = User.Identity?.IsAuthenticated == true;
        return View(item);
    }
}
