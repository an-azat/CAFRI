using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class MapController : Controller
{
    [HttpGet("/map")]
    public IActionResult Index() => Redirect("/countries");
}
