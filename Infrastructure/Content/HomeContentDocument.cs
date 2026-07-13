using CAFRI.ViewModels.Home;

namespace CAFRI.Infrastructure.Content;

public sealed class HomeContentDocument
{
    public HomePageViewModel Page { get; init; } = new();
}
