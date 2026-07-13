using CAFRI.ViewModels.Map;

namespace CAFRI.Application.Abstractions.Services;

public interface IMapContentService
{
    MapPageViewModel GetIndexPage();
}
