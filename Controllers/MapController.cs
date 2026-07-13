using CAFRI.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class MapController : Controller
{
    private readonly IMapContentService _mapContentService;

    public MapController(IMapContentService mapContentService)
    {
        _mapContentService = mapContentService;
    }

    [HttpGet("/map")]
    public IActionResult Index() => View(_mapContentService.GetIndexPage());
}
