using BusinessChronicle.Samples.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusinessChronicle.Samples.Mvc.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
