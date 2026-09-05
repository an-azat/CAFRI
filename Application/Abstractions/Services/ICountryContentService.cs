using CAFRI.ViewModels.Countries;

namespace CAFRI.Application.Abstractions.Services;

public interface ICountryContentService
{
    Task<CountriesPageViewModel> GetIndexPageAsync(CancellationToken cancellationToken = default);

    Task<CountryProfilePageViewModel?> GetDetailsAsync(string slug, CancellationToken cancellationToken = default);
}
