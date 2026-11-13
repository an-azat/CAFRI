namespace CAFRI.Domain.Content;

/// <summary>
/// Editorial and data sections of a country profile page (Economic snapshot, Macroeconomic
/// performance, Public finances, Country analysis, Monitoring priorities, Latest country
/// intelligence, Data sources, Methodology). Stored as JSON in CountryContent.ProfileJson and
/// edited through structured fields in the admin; every list drops rows left empty.
/// </summary>
public sealed class CountryProfileContent
{
    public string SnapshotCutoff { get; set; } = string.Empty;
    public List<CountryProfileKpi> Kpis { get; set; } = [];

    public string MacroSubtitle { get; set; } = string.Empty;
    public string GdpSubtitle { get; set; } = string.Empty;
    public List<CountryProfileGdpPoint> GdpHistory { get; set; } = [];
    public string GdpNote { get; set; } = string.Empty;
    public string GdpSourceUrl { get; set; } = string.Empty;
    public List<CountryProfileSignal> Signals { get; set; } = [];

    public string FiscalSubtitle { get; set; } = string.Empty;
    public List<CountryProfileFiscalRow> FiscalRows { get; set; } = [];
    public string FiscalNote { get; set; } = string.Empty;
    public string DebtValue { get; set; } = string.Empty;
    public string DebtPeriod { get; set; } = string.Empty;
    public string DebtNote { get; set; } = string.Empty;

    public string AnalysisHeading { get; set; } = string.Empty;
    public string AnalysisLead { get; set; } = string.Empty;
    public List<CountryProfileAnalysisPoint> AnalysisPoints { get; set; } = [];
    public string AnalysisFootnote { get; set; } = string.Empty;
    public string AnalysisUpdated { get; set; } = string.Empty;

    public string MonitoringHeading { get; set; } = string.Empty;
    public string MonitoringIntro { get; set; } = string.Empty;
    public List<CountryProfilePriority> Priorities { get; set; } = [];
    public string MonitoringFootnote { get; set; } = string.Empty;

    public List<CountryProfileNewsItem> News { get; set; } = [];
    public List<CountryProfileSource> Sources { get; set; } = [];
    public string Methodology { get; set; } = string.Empty;
    public string GalleryCredit { get; set; } = string.Empty;

    // Drops rows that were left blank in the admin form.
    public void RemoveEmptyRows()
    {
        Kpis = Kpis.Where(x => !string.IsNullOrWhiteSpace(x.Label) && !string.IsNullOrWhiteSpace(x.Value)).ToList();
        GdpHistory = GdpHistory.Where(x => !string.IsNullOrWhiteSpace(x.Year) && x.Value.HasValue).OrderBy(x => x.Year, StringComparer.Ordinal).ToList();
        Signals = Signals.Where(x => !string.IsNullOrWhiteSpace(x.Title)).ToList();
        FiscalRows = FiscalRows.Where(x => !string.IsNullOrWhiteSpace(x.Label) && !string.IsNullOrWhiteSpace(x.Value)).ToList();
        AnalysisPoints = AnalysisPoints.Where(x => !string.IsNullOrWhiteSpace(x.Title) || !string.IsNullOrWhiteSpace(x.Text)).ToList();
        Priorities = Priorities.Where(x => !string.IsNullOrWhiteSpace(x.Title)).ToList();
        News = News.Where(x => !string.IsNullOrWhiteSpace(x.Headline)).ToList();
        Sources = Sources.Where(x => !string.IsNullOrWhiteSpace(x.Institution)).ToList();
    }
}

public sealed class CountryProfileKpi
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    // Short change line, e.g. "↓ 1.4 pp" or "Unchanged".
    public string Change { get; set; } = string.Empty;
    // positive | negative | neutral — colours the change line.
    public string ChangeTone { get; set; } = "neutral";
    // Reference period, e.g. "Jan–Sep 2026, year-on-year".
    public string Period { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public sealed class CountryProfileGdpPoint
{
    public string Year { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    // Actual | Estimate | Forecast
    public string DataType { get; set; } = "Actual";
    public string Observation { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public sealed class CountryProfileSignal
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    // positive | watch
    public string Tone { get; set; } = "watch";
}

public sealed class CountryProfileFiscalRow
{
    public string Label { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class CountryProfileAnalysisPoint
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public sealed class CountryProfilePriority
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // High | Medium | Low
    public string Level { get; set; } = "Medium";
}

public sealed class CountryProfileNewsItem
{
    // e.g. "Monetary policy · 27 Oct 2026"
    public string TypeDate { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class CountryProfileSource
{
    public string Institution { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
