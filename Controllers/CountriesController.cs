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
    public IActionResult Index() => View(_countryContentService.GetIndexPage());

    [HttpGet("/countries/{slug}")]
    public IActionResult Details(string slug)
    {
        var model = _countryContentService.GetDetails(slug);
        return model is null ? NotFound() : View(model);
    }
}
