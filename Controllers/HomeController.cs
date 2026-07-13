using System.Diagnostics;
using CAFRI.Application.Abstractions.Services;
using CAFRI.Models;
using CAFRI.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class HomeController : Controller
{
    private readonly IHomeContentService _homeContentService;
    private readonly ICountryContentService _countryContentService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IHomeContentService homeContentService,
        ICountryContentService countryContentService,
        ILogger<HomeController> logger)
    {
        _homeContentService = homeContentService;
        _countryContentService = countryContentService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var page = _homeContentService.GetHomePage();
        var countries = _countryContentService.GetIndexPage().Countries;

        var model = new HomePageViewModel
        {
            LatestIntelligence = page.LatestIntelligence,
            FeaturedPublication = page.FeaturedPublication,
            CoverageAreas = page.CoverageAreas,
            Countries = countries
        };

        return View(model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
