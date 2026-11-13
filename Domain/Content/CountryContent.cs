namespace CAFRI.Domain.Content;

public sealed class CountryContent
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Capital { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Gdp { get; set; } = string.Empty;
    public string Population { get; set; } = string.Empty;
    public string BankAssets { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    // Lead paragraph under the country name on the profile page.
    public string ProfileLead { get; set; } = string.Empty;
    // Profile header: official name shown as the page title (falls back to Name), region label,
    // flag image and the reference period printed under each headline fact.
    public string OfficialName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string? FlagImageUrl { get; set; }
    public string PopulationPeriod { get; set; } = string.Empty;
    public string GdpPeriod { get; set; } = string.Empty;
    public string OfficialLanguages { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    // "About the country" section: paragraphs separated by blank lines, plus quick facts.
    public string AboutText { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string NeighbouringCountries { get; set; } = string.Empty;
    // Remaining profile sections (snapshot, macro, finances, analysis, news, sources...) as
    // serialized CountryProfileContent; edited through structured admin fields, never raw.
    public string? ProfileJson { get; set; }
    public string HeroClassName { get; set; } = string.Empty;
    public string? HeroGalleryJson { get; set; }
    // Legacy: the seeded profile JSON (ratings, banks, tabs...). No longer read or edited; kept
    // in the database so nothing is lost. Currency and ProfileLead were migrated out of it.
    public string DetailsJson { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
