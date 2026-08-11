using CAFRI.Areas.Admin.ViewModels.Content;
using CAFRI.Domain.Content;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Content;
using CAFRI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class PagesController : Controller
{
    private readonly AppDbContext _dbContext;

    public PagesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Home(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Home Page";
        ViewData["AdminNav"] = "pages-home";

        var entity = await _dbContext.HomePageContents
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return View(BuildEditViewModel(new HomePageContentEditViewModel()));
        }

        return View(BuildEditViewModel(entity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Home(HomePageContentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Home Page";
        ViewData["AdminNav"] = "pages-home";

        if (HomePageContentTextSerializer.ParseDirections(model.DirectionsText).Count == 0)
        {
            ModelState.AddModelError(nameof(model.DirectionsText), "Add at least one platform card.");
        }

        if (HomePageContentTextSerializer.ParseFeatureCards(model.FeatureCardsText).Count == 0)
        {
            ModelState.AddModelError(nameof(model.FeatureCardsText), "Add at least one feature card.");
        }

        if (HomePageContentTextSerializer.ParseCoverageAreas(model.CoverageAreasText).Count == 0)
        {
            ModelState.AddModelError(nameof(model.CoverageAreasText), "Add at least one coverage area.");
        }

        if (!ModelState.IsValid)
        {
            return View(BuildEditViewModel(model));
        }

        var entity = await _dbContext.HomePageContents
            .OrderByDescending(x => x.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            entity = new HomePageContent
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            _dbContext.HomePageContents.Add(entity);
        }

        entity.HeroTitle = model.HeroTitle.Trim();
        entity.HeroLead = model.HeroLead.Trim();
        entity.HeroPrimaryCtaLabel = model.HeroPrimaryCtaLabel.Trim();
        entity.HeroPrimaryCtaUrl = model.HeroPrimaryCtaUrl.Trim();
        entity.HeroSecondaryCtaLabel = model.HeroSecondaryCtaLabel.Trim();
        entity.HeroSecondaryCtaUrl = model.HeroSecondaryCtaUrl.Trim();
        entity.IntroTitle = model.IntroTitle.Trim();
        entity.IntroDescription = model.IntroDescription.Trim();
        entity.PlatformSectionLabel = model.PlatformSectionLabel.Trim();
        entity.PlatformSectionTitle = model.PlatformSectionTitle.Trim();
        entity.PlatformSectionDescription = model.PlatformSectionDescription.Trim();
        entity.DirectionsText = model.DirectionsText.Trim();
        entity.CountriesSectionLabel = model.CountriesSectionLabel.Trim();
        entity.CountriesSectionTitle = model.CountriesSectionTitle.Trim();
        entity.CountriesSectionDescription = model.CountriesSectionDescription.Trim();
        entity.CountriesVisualTitle = model.CountriesVisualTitle.Trim();
        entity.CountriesVisualDescription = model.CountriesVisualDescription.Trim();
        entity.FeaturesSectionLabel = model.FeaturesSectionLabel.Trim();
        entity.FeaturesSectionTitle = model.FeaturesSectionTitle.Trim();
        entity.FeaturesSectionDescription = model.FeaturesSectionDescription.Trim();
        entity.FeatureCardsText = model.FeatureCardsText.Trim();
        entity.LatestSectionLabel = model.LatestSectionLabel.Trim();
        entity.LatestSectionTitle = model.LatestSectionTitle.Trim();
        entity.LatestSectionDescription = model.LatestSectionDescription.Trim();
        entity.FeaturedPublicationEyebrow = model.FeaturedPublicationEyebrow.Trim();
        entity.LatestFeaturedLinkLabel = model.LatestFeaturedLinkLabel.Trim();
        entity.LatestIntelligenceLinkLabel = model.LatestIntelligenceLinkLabel.Trim();
        entity.FeaturedPublicationId = model.FeaturedPublicationId;
        entity.AboutTitle = model.AboutTitle.Trim();
        entity.AboutDescription = model.AboutDescription.Trim();
        entity.CoverageAreasText = model.CoverageAreasText.Trim();
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["AdminSuccess"] = "Home page content saved.";
        return RedirectToAction(nameof(Home));
    }

    private HomePageContentEditViewModel BuildEditViewModel(HomePageContent entity) =>
        BuildEditViewModel(new HomePageContentEditViewModel
        {
            Id = entity.Id,
            HeroTitle = entity.HeroTitle,
            HeroLead = entity.HeroLead,
            HeroPrimaryCtaLabel = entity.HeroPrimaryCtaLabel,
            HeroPrimaryCtaUrl = entity.HeroPrimaryCtaUrl,
            HeroSecondaryCtaLabel = entity.HeroSecondaryCtaLabel,
            HeroSecondaryCtaUrl = entity.HeroSecondaryCtaUrl,
            IntroTitle = entity.IntroTitle,
            IntroDescription = entity.IntroDescription,
            PlatformSectionLabel = entity.PlatformSectionLabel,
            PlatformSectionTitle = entity.PlatformSectionTitle,
            PlatformSectionDescription = entity.PlatformSectionDescription,
            DirectionsText = entity.DirectionsText,
            CountriesSectionLabel = entity.CountriesSectionLabel,
            CountriesSectionTitle = entity.CountriesSectionTitle,
            CountriesSectionDescription = entity.CountriesSectionDescription,
            CountriesVisualTitle = entity.CountriesVisualTitle,
            CountriesVisualDescription = entity.CountriesVisualDescription,
            FeaturesSectionLabel = entity.FeaturesSectionLabel,
            FeaturesSectionTitle = entity.FeaturesSectionTitle,
            FeaturesSectionDescription = entity.FeaturesSectionDescription,
            FeatureCardsText = entity.FeatureCardsText,
            LatestSectionLabel = entity.LatestSectionLabel,
            LatestSectionTitle = entity.LatestSectionTitle,
            LatestSectionDescription = entity.LatestSectionDescription,
            FeaturedPublicationEyebrow = entity.FeaturedPublicationEyebrow,
            LatestFeaturedLinkLabel = entity.LatestFeaturedLinkLabel,
            LatestIntelligenceLinkLabel = entity.LatestIntelligenceLinkLabel,
            FeaturedPublicationId = entity.FeaturedPublicationId,
            AboutTitle = entity.AboutTitle,
            AboutDescription = entity.AboutDescription,
            CoverageAreasText = entity.CoverageAreasText
        });

    private HomePageContentEditViewModel BuildEditViewModel(HomePageContentEditViewModel model) =>
        new()
        {
            Id = model.Id,
            HeroTitle = model.HeroTitle,
            HeroLead = model.HeroLead,
            HeroPrimaryCtaLabel = model.HeroPrimaryCtaLabel,
            HeroPrimaryCtaUrl = model.HeroPrimaryCtaUrl,
            HeroSecondaryCtaLabel = model.HeroSecondaryCtaLabel,
            HeroSecondaryCtaUrl = model.HeroSecondaryCtaUrl,
            IntroTitle = model.IntroTitle,
            IntroDescription = model.IntroDescription,
            PlatformSectionLabel = model.PlatformSectionLabel,
            PlatformSectionTitle = model.PlatformSectionTitle,
            PlatformSectionDescription = model.PlatformSectionDescription,
            DirectionsText = model.DirectionsText,
            CountriesSectionLabel = model.CountriesSectionLabel,
            CountriesSectionTitle = model.CountriesSectionTitle,
            CountriesSectionDescription = model.CountriesSectionDescription,
            CountriesVisualTitle = model.CountriesVisualTitle,
            CountriesVisualDescription = model.CountriesVisualDescription,
            FeaturesSectionLabel = model.FeaturesSectionLabel,
            FeaturesSectionTitle = model.FeaturesSectionTitle,
            FeaturesSectionDescription = model.FeaturesSectionDescription,
            FeatureCardsText = model.FeatureCardsText,
            LatestSectionLabel = model.LatestSectionLabel,
            LatestSectionTitle = model.LatestSectionTitle,
            LatestSectionDescription = model.LatestSectionDescription,
            FeaturedPublicationEyebrow = model.FeaturedPublicationEyebrow,
            LatestFeaturedLinkLabel = model.LatestFeaturedLinkLabel,
            LatestIntelligenceLinkLabel = model.LatestIntelligenceLinkLabel,
            FeaturedPublicationId = model.FeaturedPublicationId,
            AvailablePublications = _dbContext.PublicationContentItems
                .AsNoTracking()
                .Where(x => x.IsPublished)
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new AdminPublicationOptionViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Meta = $"{x.Type} | {x.PublishedDate}"
                })
                .ToList(),
            AboutTitle = model.AboutTitle,
            AboutDescription = model.AboutDescription,
            CoverageAreasText = model.CoverageAreasText
        };
}
