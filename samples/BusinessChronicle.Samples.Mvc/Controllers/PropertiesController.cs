using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.AspNetCore.Services;
using BusinessChronicle.Samples.Shared.Models;
using BusinessChronicle.Samples.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusinessChronicle.Samples.Mvc.Controllers;

public sealed class PropertiesController(PropertyUnitCatalog catalog, IChronicleApiService chronicle) : Controller
{
    public IActionResult Index() => View(catalog.GetUnits());

    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        PropertyUnit? unit = catalog.GetUnit(id);
        if (unit is null)
        {
            return NotFound();
        }

        EntityReference entity = unit.ToEntityReference();
        var timelineResult = await chronicle
            .GetTimelineAsync(entity, new TimelineQueryOptions { MaxResults = 20, Descending = false }, cancellationToken)
            .ConfigureAwait(false);

        ViewBag.Timeline = timelineResult.IsSuccess ? timelineResult.Value! : Array.Empty<TimelineEntry>();
        return View(unit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Advance(string id, CancellationToken cancellationToken)
    {
        _ = await catalog.AdvanceUnitAsync(id, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { id });
    }
}
