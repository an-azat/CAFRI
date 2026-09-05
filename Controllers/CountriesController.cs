using CAFRI.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class CountriesController : Controller
{
    private readonly ICountryContentService _countryContentService;

    public CountriesController(ICountryContentService countryContentService)
    {
        _countryContentService = countryContentService;
    }

    [HttpGet("/countries")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _countryContentService.GetIndexPageAsync(cancellationToken));

    [HttpGet("/countries/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var model = await _countryContentService.GetDetailsAsync(slug, cancellationToken);
        return model is null ? NotFound() : View(model);
    }
}
