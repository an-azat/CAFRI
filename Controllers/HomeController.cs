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

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _homeContentService.GetHomePageAsync(cancellationToken);
        var countries = (await _countryContentService.GetIndexPageAsync(cancellationToken)).Countries;

        var model = new HomePageViewModel
        {
            HeroTitle = page.HeroTitle,
            HeroLead = page.HeroLead,
            HeroPrimaryCtaLabel = page.HeroPrimaryCtaLabel,
            HeroPrimaryCtaUrl = page.HeroPrimaryCtaUrl,
            HeroSecondaryCtaLabel = page.HeroSecondaryCtaLabel,
            HeroSecondaryCtaUrl = page.HeroSecondaryCtaUrl,
            IntroTitle = page.IntroTitle,
            IntroDescription = page.IntroDescription,
            PlatformSectionLabel = page.PlatformSectionLabel,
            PlatformSectionTitle = page.PlatformSectionTitle,
            PlatformSectionDescription = page.PlatformSectionDescription,
            Directions = page.Directions,
            CountriesSectionLabel = page.CountriesSectionLabel,
            CountriesSectionTitle = page.CountriesSectionTitle,
            CountriesSectionDescription = page.CountriesSectionDescription,
            CountriesVisualTitle = page.CountriesVisualTitle,
            CountriesVisualDescription = page.CountriesVisualDescription,
            FeaturesSectionLabel = page.FeaturesSectionLabel,
            FeaturesSectionTitle = page.FeaturesSectionTitle,
            FeaturesSectionDescription = page.FeaturesSectionDescription,
            FeatureCards = page.FeatureCards,
            LatestSectionLabel = page.LatestSectionLabel,
            LatestSectionTitle = page.LatestSectionTitle,
            LatestSectionDescription = page.LatestSectionDescription,
            FeaturedPublicationEyebrow = page.FeaturedPublicationEyebrow,
            LatestFeaturedLinkLabel = page.LatestFeaturedLinkLabel,
            LatestIntelligenceLinkLabel = page.LatestIntelligenceLinkLabel,
            AboutTitle = page.AboutTitle,
            AboutDescription = page.AboutDescription,
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
