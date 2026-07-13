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
    public IActionResult Index(
        string? search,
        string? country,
        string? category,
        string? institution,
        string? dateRange,
        string? sortBy,
        int page = 1) =>
        View(_intelligenceContentService.GetIndexPage(search, country, category, institution, dateRange, sortBy, page));

    [HttpGet("/intelligence/{slug}")]
    public IActionResult Details(string slug)
    {
        var item = _intelligenceContentService.GetDetails(slug);
        if (item is null)
        {
            return NotFound();
        }

        item.CanAccessFullContent = User.Identity?.IsAuthenticated == true;
        return View(item);
    }
}
