namespace CAFRI.ViewModels.Countries;

// Single place for the country code -> flag icon mapping.
public static class CountryFlags
{
    public static string IconUrl(string? countryCode) => countryCode?.ToUpperInvariant() switch
    {
        "KZ" => "/uploads/img/icons8-казахстан-48.png",
        "UZ" => "/uploads/img/icons8-узбекистан-48.png",
        "KG" => "/uploads/img/icons8-киргизия-48.png",
        "TJ" => "/uploads/img/icons8-таджикистан-48.png",
        "TM" => "/uploads/img/icons8-туркменистан-циркуляр-48.png",
        _ => string.Empty
    };
}
