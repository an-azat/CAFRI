using CAFRI.ViewModels.Countries;

namespace CAFRI.Application.Abstractions.Services;

public interface ICountryContentService
{
    CountriesPageViewModel GetIndexPage();

    CountryProfilePageViewModel? GetDetails(string slug);
}
