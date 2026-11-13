using CAFRI.Domain.Content;

namespace CAFRI.Infrastructure.Content;

/// <summary>
/// Country profiles and indicators as of end-2025, compiled on 14 Nov 2026 from:
/// World Bank WDI (real GDP growth 2020–2025, population 2025, GDP current US$ 2025; updated
/// 11 Nov 2026), IMF World Economic Outlook (general government gross debt 2025), national
/// statistics offices and central banks (December 2025 inflation, policy rates at end-2025),
/// and the IMF 2026 Article IV statement for Turkmenistan. Used by the
/// ReplaceCountryDataWithEnd2025 migration and by ContentSeedService for new databases.
/// </summary>
public static class CountryReferenceData2025
{
    public const string CutoffLabel = "31 December 2025";
    private const string AnalysisDate = "14 Nov 2026";
    private const string WorldBankWdi = "World Bank WDI";
    private const string ImfWeo = "IMF World Economic Outlook";

    public sealed record CountryFacts(
        string Code,
        string OfficialName,
        string Summary,
        string ProfileLead,
        string AboutText,
        string Capital,
        string Currency,
        string OfficialLanguages,
        string Area,
        string NeighbouringCountries,
        string Population,
        string Gdp);

    public sealed record IndicatorRow(
        string CountryCode,
        string Name,
        string Slug,
        string Category,
        string Unit,
        string LatestValue,
        string YearLabel,
        string SourceName,
        string Notes);

    public const string PopulationPeriod = "2025 · World Bank WDI";
    public const string GdpPeriod = "2025 · current US$ · World Bank WDI";
    public const string DataSource = "World Bank, World Development Indicators (updated 11 Nov 2026)";

    public static readonly IReadOnlyList<CountryFacts> Countries =
    [
        new("KZ", "Republic of Kazakhstan",
            "Largest economy in Central Asia, driven by hydrocarbons, mining and a growing services sector.",
            "Kazakhstan is the largest economy in Central Asia, with growth in 2025 supported by higher oil output, strong domestic demand and a tight monetary policy stance against elevated inflation.",
            "Kazakhstan is the world's largest landlocked country, stretching from the Caspian Sea to the Altai mountains and bordering Russia, China, Kyrgyzstan, Uzbekistan and Turkmenistan.\n\nThe economy is anchored in oil, gas and metals, with a growing role for transport, trade and financial services. Hydrocarbon output, the tenge exchange rate and inflation are the main factors shaping macroeconomic developments.",
            "Astana", "Kazakhstani tenge (KZT)", "Kazakh · Russian", "2,724,900 km²",
            "Russia, China, Kyrgyzstan, Uzbekistan, Turkmenistan", "20.84 million", "US$306.2 bn"),
        new("UZ", "Republic of Uzbekistan",
            "Most populous country in Central Asia, with fast growth and an ongoing market reform programme.",
            "Uzbekistan is the most populous country in Central Asia. Its economy grew 7.7% in 2025, supported by investment, remittances and gold exports, while inflation continued to slow.",
            "Uzbekistan is a doubly landlocked country bordering Kazakhstan, Kyrgyzstan, Tajikistan, Afghanistan and Turkmenistan, with the Fergana Valley and the Tashkent region as its main population centres.\n\nThe economy combines gold, natural gas, cotton and textiles, construction and a fast-expanding services sector. Structural reforms, privatisation and the shift towards a market-based exchange rate have shaped recent growth.",
            "Tashkent", "Uzbek som (UZS)", "Uzbek", "448,978 km²",
            "Kazakhstan, Kyrgyzstan, Tajikistan, Afghanistan, Turkmenistan", "37.05 million", "US$147.0 bn"),
        new("KG", "Kyrgyz Republic",
            "Open, mountainous economy with strong growth, remittance inflows and re-export trade.",
            "The Kyrgyz Republic is a landlocked Central Asian country in the Tian Shan mountains. Its economy grew 11.1% in 2025, while inflation pressures led the National Bank to raise its policy rate.",
            "The Kyrgyz Republic is a landlocked country bordered by Kazakhstan, Uzbekistan, Tajikistan and China. Its territory is dominated by the Tian Shan mountain system, while the Chüy and Fergana valleys host the main population centres.\n\nThe economy combines services, trade, construction, agriculture, gold mining and hydropower. Remittances, cross-border trade within the Eurasian Economic Union and external demand are key factors shaping economic developments.",
            "Bishkek", "Kyrgyz som (KGS)", "Kyrgyz · Russian", "199,951 km²",
            "Kazakhstan, Uzbekistan, Tajikistan, China", "7.34 million", "US$22.6 bn"),
        new("TJ", "Republic of Tajikistan",
            "Remittance-dependent economy with high growth, low inflation and large hydropower potential.",
            "Tajikistan is a mountainous Central Asian economy that grew 8.4% in 2025 for a second consecutive year, with inflation near the lower end of the National Bank's target range.",
            "Tajikistan is a landlocked country bordering Uzbekistan, Kyrgyzstan, China and Afghanistan; more than nine-tenths of its territory is mountainous, including much of the Pamir range.\n\nThe economy relies on remittances from labour migrants, aluminium, cotton, mining and hydropower. Rising remittance inflows and public investment have supported growth, while the banking sector remains small.",
            "Dushanbe", "Tajikistani somoni (TJS)", "Tajik", string.Empty,
            "Uzbekistan, Kyrgyzstan, China, Afghanistan", "10.79 million", "US$17.7 bn"),
        new("TM", "Turkmenistan",
            "Gas-rich, state-led economy with a fixed exchange rate and limited published data.",
            "Turkmenistan holds some of the world's largest natural gas reserves. Official data put 2025 growth at 6.3%, while IMF staff estimate it at 3.6%; public debt remains very low.",
            "Turkmenistan lies between the Caspian Sea and the Amu Darya river and borders Kazakhstan, Uzbekistan, Afghanistan and Iran; most of its territory is covered by the Karakum desert.\n\nThe economy is dominated by natural gas production and exports, alongside oil, chemicals, cotton and construction. The state plays a central role, the manat is officially fixed to the US dollar, and access to official statistics is limited.",
            "Ashgabat", "Turkmen manat (TMT)", "Turkmen", "491,210 km²",
            "Kazakhstan, Uzbekistan, Afghanistan, Iran", "7.62 million", "US$49.8 bn")
    ];

    // Real GDP growth, % (World Bank WDI, updated 11 Nov 2026). 2025 values are preliminary.
    private static readonly Dictionary<string, decimal[]> GdpGrowth2020To2025 = new()
    {
        ["KZ"] = [-2.5m, 4.3m, 3.2m, 5.1m, 5.0m, 6.5m],
        ["UZ"] = [1.6m, 8.2m, 6.1m, 6.3m, 6.7m, 7.7m],
        ["KG"] = [-7.1m, 5.5m, 9.0m, 9.0m, 11.5m, 11.1m],
        ["TJ"] = [4.4m, 9.4m, 8.0m, 8.3m, 8.4m, 8.4m],
        ["TM"] = [5.9m, 6.2m, 6.2m, 6.3m, 6.3m, 6.3m]
    };

    public static CountryProfileContent BuildProfile(string code)
    {
        var profile = code switch
        {
            "KZ" => Kazakhstan(),
            "UZ" => Uzbekistan(),
            "KG" => Kyrgyzstan(),
            "TJ" => Tajikistan(),
            "TM" => Turkmenistan(),
            _ => new CountryProfileContent()
        };

        profile.SnapshotCutoff = CutoffLabel;
        profile.GdpHistory = GdpHistory(code);
        profile.GdpSubtitle = "Annual change, % · 2020–2025 · World Bank, World Development Indicators";
        profile.AnalysisUpdated = AnalysisDate;
        profile.MonitoringHeading = "Key areas to watch";
        profile.MonitoringIntro = "Topics selected by CAFRI for continued country monitoring.";
        profile.MonitoringFootnote = "Priorities reflect CAFRI's editorial monitoring focus, not official risk ratings.";
        profile.Methodology = "Indicators are shown with their own reference periods and source definitions, with a data cut-off of 31 December 2025. GDP growth for 2025 is preliminary. Inflation is shown year-on-year for December where the national source publishes it. Public debt is IMF general government gross debt and may differ from national ministry figures because of coverage and methodology. Analytical labels are CAFRI editorial assessments, not official ratings.";
        return profile;
    }

    private static List<CountryProfileGdpPoint> GdpHistory(string code) =>
        GdpGrowth2020To2025.TryGetValue(code, out var values)
            ? values.Select((value, index) => new CountryProfileGdpPoint
            {
                Year = (2020 + index).ToString(System.Globalization.CultureInfo.InvariantCulture),
                Value = value,
                DataType = index == values.Length - 1 ? "Estimate" : "Actual",
                Observation = value < 0 ? "Contraction" : index > 0 && values[index - 1] < 0 ? "Recovery" : value >= 8 ? "High growth" : "Expansion",
                Source = WorldBankWdi
            }).ToList()
            : [];

    private static CountryProfileContent Kazakhstan() => new()
    {
        Kpis =
        [
            new() { Label = "Real GDP growth", Value = "6.5%", Change = "↑ 1.5 pp vs 2024", ChangeTone = "positive", Period = "2025, preliminary", Source = WorldBankWdi },
            new() { Label = "Annual inflation", Value = "12.3%", Change = "↑ 3.7 pp vs 2024", ChangeTone = "negative", Period = "December 2025, year-on-year", Source = "Bureau of National Statistics" },
            new() { Label = "Base rate", Value = "18.0%", Change = "↑ 1.5 pp on 10 Oct 2025", ChangeTone = "neutral", Period = "As of 31 Dec 2025", Source = "National Bank of Kazakhstan" },
            new() { Label = "Public debt", Value = "24.3%", Change = "↓ 0.1 pp vs 2024", ChangeTone = "neutral", Period = "of GDP · 2025, general government", Source = ImfWeo }
        ],
        MacroSubtitle = "Growth accelerated in 2025 while inflation stayed elevated",
        GdpNote = "Annual real GDP growth, year-on-year. The 2025 value is preliminary.",
        GdpSourceUrl = "https://data.worldbank.org/indicator/NY.GDP.MKTP.KD.ZG?locations=KZ",
        Signals =
        [
            new() { Title = "Fastest growth in over a decade", Text = "Real GDP grew 6.5% in 2025, supported by higher oil output and strong domestic demand.", Tone = "positive" },
            new() { Title = "Inflation remains elevated", Text = "Annual inflation was 12.3% in December 2025, up from 8.6% a year earlier and well above the National Bank's target." },
            new() { Title = "Tight monetary stance", Text = "The National Bank raised the base rate to 18% in October 2025 and kept it unchanged at its November meeting." }
        ],
        DebtValue = "24.3%",
        DebtPeriod = "2025",
        DebtNote = "IMF estimate of general government gross debt. National definitions of public debt can differ.",
        AnalysisHeading = "Macroeconomic assessment",
        AnalysisLead = "Kazakhstan combined strong growth with persistent inflation in 2025, keeping monetary policy tight going into 2026.",
        AnalysisPoints =
        [
            new() { Title = "Economic momentum", Text = "Real GDP grew 6.5% in 2025, the strongest pace in more than a decade, helped by the expansion of oil production and household demand." },
            new() { Title = "Price stability", Text = "Annual inflation reached 12.3% in December 2025; food prices rose 13.5% over the year, keeping inflation far above target." },
            new() { Title = "Policy response", Text = "With the base rate at 18%, monetary conditions are restrictive; the pace of disinflation will shape the timing of any easing." }
        ],
        AnalysisFootnote = "Based on Bureau of National Statistics, National Bank of Kazakhstan, World Bank and IMF data. This is CAFRI editorial analysis, not an official country rating.",
        Priorities =
        [
            new() { Title = "Inflation & monetary policy", Description = "Consumer prices, inflation expectations and base-rate decisions", Level = "High" },
            new() { Title = "Oil sector & exports", Description = "Oil output, export volumes and their effect on growth and the tenge", Level = "High" },
            new() { Title = "Banking & consumer credit", Description = "Retail lending growth, asset quality and macroprudential measures", Level = "Medium" },
            new() { Title = "Public finances", Description = "Budget execution, National Fund transfers and tax reform", Level = "Medium" }
        ],
        News =
        [
            new() { TypeDate = "Official statistics · 05 Jan 2026", Headline = "Inflation in Kazakhstan was 12.3% in 2025", Summary = "Bureau of National Statistics release on annual and December consumer price inflation.", Url = "https://stat.gov.kz/ru/news/inflyatsiya-v-respublike-kazakhstan-v-2025-godu/" },
            new() { TypeDate = "Monetary policy · 28 Nov 2025", Headline = "National Bank of Kazakhstan keeps the base rate at 18%", Summary = "Rate decision following the 150 bp increase in October 2025.", Url = "https://interfax.com/newsroom/top-stories/115048/" }
        ],
        Sources =
        [
            new() { Institution = "Bureau of National Statistics", Description = "Inflation, GDP and official economic statistics", Url = "https://stat.gov.kz/" },
            new() { Institution = "National Bank of Kazakhstan", Description = "Base rate decisions and monetary policy reports", Url = "https://nationalbank.kz/" },
            new() { Institution = "World Bank", Description = "World Development Indicators: GDP growth, population, GDP in US$", Url = "https://data.worldbank.org/country/kazakhstan" },
            new() { Institution = "International Monetary Fund", Description = "World Economic Outlook: general government debt", Url = "https://www.imf.org/en/Countries/KAZ" }
        ]
    };

    private static CountryProfileContent Uzbekistan() => new()
    {
        Kpis =
        [
            new() { Label = "Real GDP growth", Value = "7.7%", Change = "↑ 1.0 pp vs 2024", ChangeTone = "positive", Period = "2025, preliminary", Source = WorldBankWdi },
            new() { Label = "Annual inflation", Value = "7.3%", Change = "Core inflation 5.7%", ChangeTone = "positive", Period = "December 2025, year-on-year", Source = "Central Bank of Uzbekistan" },
            new() { Label = "Policy rate", Value = "14.0%", Change = "Unchanged since March 2025", ChangeTone = "neutral", Period = "As of 31 Dec 2025", Source = "Central Bank of Uzbekistan" },
            new() { Label = "Public debt", Value = "28.6%", Change = "↓ 2.3 pp vs 2024", ChangeTone = "positive", Period = "of GDP · 2025, general government", Source = ImfWeo }
        ],
        MacroSubtitle = "Strong growth with gradual disinflation",
        GdpNote = "Annual real GDP growth, year-on-year. The 2025 value is preliminary.",
        GdpSourceUrl = "https://data.worldbank.org/indicator/NY.GDP.MKTP.KD.ZG?locations=UZ",
        Signals =
        [
            new() { Title = "Growth above expectations", Text = "Real GDP grew 7.7% in 2025, exceeding the Central Bank's early-year projection of about 6%.", Tone = "positive" },
            new() { Title = "Disinflation continues", Text = "Headline inflation slowed to 7.3% in December 2025 and core inflation to 5.7%, against a 5% medium-term target.", Tone = "positive" },
            new() { Title = "Restrictive policy rate", Text = "The policy rate has stayed at 14% since March 2025; the IMF urged continued fiscal restraint." }
        ],
        DebtValue = "28.6%",
        DebtPeriod = "2025",
        DebtNote = "IMF estimate of general government gross debt. The Ministry of Economy and Finance reported public debt of US$46.85 billion, 31.9% of GDP, at end-2025.",
        AnalysisHeading = "Macroeconomic assessment",
        AnalysisLead = "Uzbekistan's growth accelerated in 2025 while inflation eased, giving the Central Bank room to keep a steady, restrictive stance.",
        AnalysisPoints =
        [
            new() { Title = "Economic momentum", Text = "Real GDP grew 7.7% in 2025, supported by investment, remittances and services." },
            new() { Title = "Price stability", Text = "Inflation fell to 7.3% by December 2025; the Central Bank projects about 6.5% at end-2026." },
            new() { Title = "Debt and fiscal position", Text = "Public debt declined relative to GDP, but external borrowing and fiscal spending remain areas to monitor." }
        ],
        AnalysisFootnote = "Based on Central Bank of Uzbekistan, Ministry of Economy and Finance, World Bank and IMF data. This is CAFRI editorial analysis, not an official country rating.",
        Priorities =
        [
            new() { Title = "Inflation & monetary policy", Description = "Path to the 5% inflation target and policy-rate decisions", Level = "High" },
            new() { Title = "External debt", Description = "External borrowing, debt service and Eurobond issuance", Level = "High" },
            new() { Title = "Banking reform", Description = "State bank privatisation, credit growth and asset quality", Level = "Medium" },
            new() { Title = "Trade & remittances", Description = "Gold exports, remittance inflows and regional trade", Level = "Medium" }
        ],
        News =
        [
            new() { TypeDate = "Public finances · 2026", Headline = "Uzbekistan's public debt reached US$46.9 billion at end-2025", Summary = "Ministry of Economy and Finance data: 31.9% of GDP, of which external debt US$39.8 billion.", Url = "https://uzdaily.ru/en/uzbekistans-public-debt-reaches-us469-billion-by-end-of-2025/" },
            new() { TypeDate = "Country surveillance · 27 Nov 2025", Headline = "IMF says Uzbekistan's economy remains robust, urges restraint in fiscal spending", Summary = "IMF staff assessment of growth, inflation and fiscal policy.", Url = "https://kun.uz/en/news/2025/11/27/imf-says-uzbekistans-economy-remains-robust-urges-restraint-in-fiscal-spending" }
        ],
        Sources =
        [
            new() { Institution = "National Statistics Committee", Description = "GDP, population and official economic statistics", Url = "https://stat.uz/" },
            new() { Institution = "Central Bank of Uzbekistan", Description = "Inflation, policy rate and monetary policy statements", Url = "https://cbu.uz/" },
            new() { Institution = "World Bank", Description = "World Development Indicators: GDP growth, population, GDP in US$", Url = "https://data.worldbank.org/country/uzbekistan" },
            new() { Institution = "International Monetary Fund", Description = "World Economic Outlook: general government debt", Url = "https://www.imf.org/en/Countries/UZB" }
        ]
    };

    private static CountryProfileContent Kyrgyzstan() => new()
    {
        Kpis =
        [
            new() { Label = "Real GDP growth", Value = "11.1%", Change = "↓ 0.4 pp vs 2024", ChangeTone = "neutral", Period = "2025, preliminary", Source = WorldBankWdi },
            new() { Label = "Annual inflation", Value = "9.4%", Change = "Above the 5–7% target", ChangeTone = "negative", Period = "End of 2025, year-on-year", Source = "National Bank of the Kyrgyz Republic" },
            new() { Label = "Policy rate", Value = "11.0%", Change = "↑ 1.0 pp on 25 Nov 2025", ChangeTone = "neutral", Period = "As of 31 Dec 2025", Source = "National Bank of the Kyrgyz Republic" },
            new() { Label = "Public debt", Value = "39.4%", Change = "↑ 3.2 pp vs 2024", ChangeTone = "negative", Period = "of GDP · 2025, general government", Source = ImfWeo }
        ],
        MacroSubtitle = "Double-digit growth for a second year, with rising inflation",
        GdpNote = "Annual real GDP growth, year-on-year. The 2025 value is preliminary.",
        GdpSourceUrl = "https://data.worldbank.org/indicator/NY.GDP.MKTP.KD.ZG?locations=KG",
        Signals =
        [
            new() { Title = "Strong domestic demand", Text = "Real GDP grew 11.1% in 2025, with domestic demand as the main contributor.", Tone = "positive" },
            new() { Title = "Inflation picking up", Text = "Annual inflation reached 9.4% at the end of 2025, above the National Bank's 5–7% medium-term target." },
            new() { Title = "Monetary tightening", Text = "The National Bank raised the policy rate by 100 bp to 11% from 25 November 2025." }
        ],
        DebtValue = "39.4%",
        DebtPeriod = "2025",
        DebtNote = "IMF estimate of general government gross debt. Ministry of Finance figures use a different coverage and reporting date.",
        AnalysisHeading = "Macroeconomic assessment",
        AnalysisLead = "The Kyrgyz Republic continued to grow at a double-digit pace in 2025, while rising inflation led the National Bank to tighten policy.",
        AnalysisPoints =
        [
            new() { Title = "Economic momentum", Text = "Real GDP grew 11.1% in 2025 after 11.5% in 2024, driven by construction, trade and consumption." },
            new() { Title = "Price stability", Text = "Inflation rose to 9.4% at end-2025, prompting a 100 bp increase in the policy rate to 11%." },
            new() { Title = "Debt trajectory", Text = "General government debt edged up to about 39% of GDP in 2025, making fiscal discipline important as growth normalises." }
        ],
        AnalysisFootnote = "Based on National Statistical Committee, National Bank of the Kyrgyz Republic, World Bank and IMF data. This is CAFRI editorial analysis, not an official country rating.",
        Priorities =
        [
            new() { Title = "Inflation & monetary policy", Description = "Consumer prices, inflation expectations and NBKR decisions", Level = "High" },
            new() { Title = "Banking sector", Description = "Credit growth, asset quality, liquidity and financial stability", Level = "High" },
            new() { Title = "Sanctions & re-export trade", Description = "Secondary-sanctions exposure and cross-border payments", Level = "High" },
            new() { Title = "Public finances & debt", Description = "Budget execution, debt dynamics and financing needs", Level = "Medium" }
        ],
        News =
        [
            new() { TypeDate = "Monetary policy · 2025", Headline = "National Bank of Kyrgyzstan reports rising inflation amid strong economic growth", Summary = "NBKR assessment of price dynamics and the economic outlook.", Url = "https://centralasianlight.org/news/national-bank-of-kyrgyzstan-reports-rising-inflation-amid-strong-economic-growth" },
            new() { TypeDate = "Macroeconomic outlook · 2025", Headline = "National Bank raises Kyrgyzstan's 2025 GDP growth forecast", Summary = "Updated NBKR growth projection for 2025.", Url = "https://24.kg/english/342272_National_Bank_raises_Kyrgyzstans_2025_GDP_growth_forecast" }
        ],
        Sources =
        [
            new() { Institution = "National Statistical Committee", Description = "GDP, population and official economic statistics", Url = "https://www.stat.gov.kg/" },
            new() { Institution = "National Bank of the Kyrgyz Republic", Description = "Policy rate, inflation and monetary policy statements", Url = "https://www.nbkr.kg/" },
            new() { Institution = "World Bank", Description = "World Development Indicators: GDP growth, population, GDP in US$", Url = "https://data.worldbank.org/country/kyrgyz-republic" },
            new() { Institution = "International Monetary Fund", Description = "World Economic Outlook: general government debt", Url = "https://www.imf.org/en/Countries/KGZ" }
        ]
    };

    private static CountryProfileContent Tajikistan() => new()
    {
        Kpis =
        [
            new() { Label = "Real GDP growth", Value = "8.4%", Change = "Same pace as 2024", ChangeTone = "positive", Period = "2025, preliminary", Source = WorldBankWdi },
            new() { Label = "Annual inflation", Value = "3.5%", Change = "Lower end of the 5% ± 2 pp target", ChangeTone = "positive", Period = "December 2025, year-on-year", Source = "National Bank of Tajikistan" },
            new() { Label = "Refinancing rate", Value = "7.5%", Change = "↓ 0.25 pp on 29 Oct 2025", ChangeTone = "neutral", Period = "As of 31 Dec 2025", Source = "National Bank of Tajikistan" },
            new() { Label = "Public debt", Value = "18.6%", Change = "↓ 6.3 pp vs 2024", ChangeTone = "positive", Period = "of GDP · 2025, general government", Source = ImfWeo }
        ],
        MacroSubtitle = "High growth with low inflation and falling debt",
        GdpNote = "Annual real GDP growth, year-on-year. The 2025 value is preliminary.",
        GdpSourceUrl = "https://data.worldbank.org/indicator/NY.GDP.MKTP.KD.ZG?locations=TJ",
        Signals =
        [
            new() { Title = "Sustained high growth", Text = "Real GDP grew 8.4% in 2025, the same pace as in 2024.", Tone = "positive" },
            new() { Title = "Low, stable inflation", Text = "Inflation was 3.5% in 2025, at the lower end of the National Bank's 5% ± 2 pp target range.", Tone = "positive" },
            new() { Title = "Gradual policy easing", Text = "The National Bank cut the refinancing rate in steps from 9% in early 2025 to 7.5% in October 2025." }
        ],
        DebtValue = "18.6%",
        DebtPeriod = "2025",
        DebtNote = "IMF estimate of general government gross debt. Ministry of Finance figures can differ because of coverage and exchange-rate effects.",
        AnalysisHeading = "Macroeconomic assessment",
        AnalysisLead = "Tajikistan kept growth above 8% in 2025 with low inflation, allowing the National Bank to ease policy while public debt fell relative to GDP.",
        AnalysisPoints =
        [
            new() { Title = "Economic momentum", Text = "Real GDP grew 8.4% for a second year, supported by remittances, industry and public investment." },
            new() { Title = "Price stability", Text = "Inflation of 3.5% in 2025 gave room for gradual cuts in the refinancing rate to 7.5%." },
            new() { Title = "External dependence", Text = "Reliance on migrant remittances and commodity exports keeps the economy exposed to conditions in partner countries." }
        ],
        AnalysisFootnote = "Based on National Bank of Tajikistan, Agency on Statistics, World Bank and IMF data. This is CAFRI editorial analysis, not an official country rating.",
        Priorities =
        [
            new() { Title = "Remittances & external sector", Description = "Remittance inflows, exchange rate and current account", Level = "High" },
            new() { Title = "Banking sector", Description = "Bank capitalisation, dollarisation and credit growth", Level = "Medium" },
            new() { Title = "Monetary policy", Description = "Refinancing-rate decisions and inflation within the target range", Level = "Medium" },
            new() { Title = "Energy & infrastructure", Description = "Rogun hydropower project financing and electricity exports", Level = "Medium" }
        ],
        News =
        [
            new() { TypeDate = "Inflation · 12 Feb 2026", Headline = "Tajikistan's inflation rate stands at 3.5% in 2025", Summary = "National Bank of Tajikistan figures on annual inflation.", Url = "https://www.thestar.com.my/news/world/2026/02/12/tajikistan039s-inflation-rate-stands-at-35-pct-in-2025" },
            new() { TypeDate = "Economic growth · 20 Feb 2026", Headline = "Tajikistan leads regional growth with 8.4% GDP increase", Summary = "Overview of 2025 growth drivers and inflation.", Url = "https://kz.kursiv.media/en/2026-02-20/engk-yeri-tajikistan-leads-regional-growth-with-8-4-gdp-surge/amp/" }
        ],
        Sources =
        [
            new() { Institution = "Agency on Statistics under the President", Description = "GDP, population and official economic statistics", Url = "https://www.stat.tj/" },
            new() { Institution = "National Bank of Tajikistan", Description = "Inflation, refinancing rate and monetary policy statements", Url = "https://nbt.tj/" },
            new() { Institution = "World Bank", Description = "World Development Indicators: GDP growth, population, GDP in US$", Url = "https://data.worldbank.org/country/tajikistan" },
            new() { Institution = "International Monetary Fund", Description = "World Economic Outlook: general government debt", Url = "https://www.imf.org/en/Countries/TJK" }
        ]
    };

    private static CountryProfileContent Turkmenistan() => new()
    {
        Kpis =
        [
            new() { Label = "Real GDP growth", Value = "6.3%", Change = "IMF staff estimate: 3.6%", ChangeTone = "neutral", Period = "2025, official estimate", Source = WorldBankWdi },
            new() { Label = "Annual inflation", Value = "2.7%", Change = "↓ from 4.6% in 2024", ChangeTone = "positive", Period = "2025", Source = "IMF 2026 Article IV statement" },
            new() { Label = "Policy rate", Value = "n/a", Change = "No regular policy-rate decisions are published", ChangeTone = "neutral", Period = "As of 31 Dec 2025", Source = "Central Bank of Turkmenistan" },
            new() { Label = "Public debt", Value = "3.1%", Change = "↓ 1.0 pp vs 2024", ChangeTone = "positive", Period = "of GDP · 2025, general government", Source = ImfWeo }
        ],
        MacroSubtitle = "Official growth figures differ from IMF staff estimates",
        GdpNote = "Official annual real GDP growth as reported by the World Bank. IMF staff estimate growth at 3.4% in 2024 and 3.6% in 2025, noting that official figures are hard to reconcile with other data.",
        GdpSourceUrl = "https://data.worldbank.org/indicator/NY.GDP.MKTP.KD.ZG?locations=TM",
        Signals =
        [
            new() { Title = "Data credibility gap", Text = "Official data show 6.3% growth in 2025, while IMF staff estimate 3.6%." },
            new() { Title = "Low inflation in 2025", Text = "IMF staff report inflation declining from 4.6% in 2024 to 2.7% in 2025, but expect it to rise in 2026.", Tone = "positive" },
            new() { Title = "Strong fiscal buffers", Text = "The central government budget turned to a small surplus in 2025 and public debt is about 3% of GDP.", Tone = "positive" }
        ],
        DebtValue = "3.1%",
        DebtPeriod = "2025",
        DebtNote = "IMF estimate of general government gross debt, among the lowest in the region.",
        AnalysisHeading = "Macroeconomic assessment",
        AnalysisLead = "Turkmenistan's macroeconomic position rests on gas exports and very low debt, but limited and inconsistent statistics make independent assessment difficult.",
        AnalysisPoints =
        [
            new() { Title = "Growth and data quality", Text = "Official growth of 6.3% in 2025 contrasts with an IMF staff estimate of 3.6%, supported by a smaller contraction in hydrocarbons." },
            new() { Title = "External position", Text = "The current account surplus narrowed to about 1.7% of GDP in 2025 on lower hydrocarbon prices and strong import growth." },
            new() { Title = "Fiscal buffers", Text = "A small budget surplus and public debt of about 3% of GDP leave substantial fiscal space." }
        ],
        AnalysisFootnote = "Based on the IMF 2026 Article IV statement, World Bank and official Turkmen data. This is CAFRI editorial analysis, not an official country rating.",
        Priorities =
        [
            new() { Title = "Gas exports", Description = "Export volumes to China and new pipeline routes", Level = "High" },
            new() { Title = "Data transparency", Description = "Availability and consistency of official statistics", Level = "High" },
            new() { Title = "Exchange-rate regime", Description = "Fixed manat rate, currency rationing and the parallel market", Level = "Medium" },
            new() { Title = "State-owned enterprises", Description = "SOE reform and directed lending", Level = "Medium" }
        ],
        News =
        [
            new() { TypeDate = "Country surveillance · 22 Jun 2026", Headline = "IMF staff completes 2026 Article IV mission to Turkmenistan", Summary = "IMF assessment of 2025 growth, inflation, fiscal and external positions.", Url = "https://www.imf.org/en/News/Articles/2026/06/22/pr26218-turkmenistan-imf-staff-completes-2026-article-iv-mission" },
            new() { TypeDate = "Official results · 2026", Headline = "Turkmenistan sums up the results of 2025", Summary = "Government review of 2025 economic results and objectives for 2026.", Url = "https://armenia.tmembassy.gov.tm/en/news/157765" }
        ],
        Sources =
        [
            new() { Institution = "State Committee of Turkmenistan on Statistics", Description = "Official economic statistics", Url = "https://www.stat.gov.tm/" },
            new() { Institution = "Central Bank of Turkmenistan", Description = "Exchange rate and banking sector information", Url = "https://www.cbt.tm/" },
            new() { Institution = "International Monetary Fund", Description = "Article IV statements and World Economic Outlook", Url = "https://www.imf.org/en/Countries/TKM" },
            new() { Institution = "World Bank", Description = "World Development Indicators: GDP growth, population, GDP in US$", Url = "https://data.worldbank.org/country/turkmenistan" }
        ]
    };

    // Rows for the CountryIndicators table: the GDP growth history feeds the countries map and
    // the profile chart; the 2025 values feed the Economic snapshot fallbacks.
    public static IReadOnlyList<IndicatorRow> BuildIndicators()
    {
        var rows = new List<IndicatorRow>();
        foreach (var (code, values) in GdpGrowth2020To2025)
        {
            var slugCode = code.ToLowerInvariant();
            for (var index = 0; index < values.Length; index++)
            {
                var year = (2020 + index).ToString(System.Globalization.CultureInfo.InvariantCulture);
                rows.Add(new(code, "Real GDP Growth", $"{slugCode}-real-gdp-growth-{year}", "Macroeconomics", "%",
                    values[index].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture), year, "World Bank WDI",
                    index == values.Length - 1 ? "Preliminary estimate." : string.Empty));
            }

            var profile = BuildProfile(code);
            foreach (var kpi in profile.Kpis.Where(x => !x.Label.Contains("GDP growth", StringComparison.OrdinalIgnoreCase) && x.Value != "n/a"))
            {
                var name = kpi.Label switch
                {
                    "Annual inflation" => "Inflation",
                    "Base rate" or "Refinancing rate" => "Policy rate",
                    _ => kpi.Label
                };
                rows.Add(new(code, name, $"{slugCode}-{name.ToLowerInvariant().Replace(' ', '-')}-2025", name == "Public debt" ? "Public Finance" : "Monetary Policy", "%",
                    kpi.Value.TrimEnd('%'), "2025", kpi.Source, kpi.Period));
            }
        }

        foreach (var country in Countries)
        {
            var slugCode = country.Code.ToLowerInvariant();
            rows.Add(new(country.Code, "Population", $"{slugCode}-population-2025", "Demography", "M",
                country.Population.Replace(" million", string.Empty), "2025", "World Bank WDI", string.Empty));
            rows.Add(new(country.Code, "GDP (Nominal)", $"{slugCode}-gdp-nominal-2025", "Macroeconomics", "USD B",
                country.Gdp.Replace("US$", string.Empty).Replace(" bn", string.Empty), "2025", "World Bank WDI", "Current US$."));
        }

        return rows;
    }
}
