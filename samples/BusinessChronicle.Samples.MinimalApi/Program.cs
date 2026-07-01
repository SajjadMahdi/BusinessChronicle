using BusinessChronicle.AspNetCore.DependencyInjection;
using BusinessChronicle.AspNetCore.Endpoints;
using BusinessChronicle.AspNetCore.OpenApi;
using BusinessChronicle.Samples.Shared.Models;
using BusinessChronicle.Samples.Shared.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddBusinessChronicle(static options => options.Entity.AllowRollback = true);
builder.Services.AddPropertyUnitCatalog();
builder.Services.AddBusinessChronicleOpenApi(options =>
{
    options.OpenApiTitle = "Chronicle ERP — Property Portfolio API";
});

WebApplication app = builder.Build();

PropertyUnitCatalog catalog = app.Services.GetRequiredService<PropertyUnitCatalog>();
await catalog.EnsureSeededAsync();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapBusinessChronicle();
app.MapBusinessChronicleScalar();

app.MapGet("/api/demo/units", () => Results.Ok(catalog.GetUnits()));
app.MapGet("/api/demo/units/{id}", (string id) =>
{
    PropertyUnit? unit = catalog.GetUnit(id);
    return unit is null ? Results.NotFound() : Results.Ok(unit);
});

app.MapPost("/api/demo/units/{id}/advance", async (string id, CancellationToken cancellationToken) =>
{
    var result = await catalog.AdvanceUnitAsync(id, cancellationToken).ConfigureAwait(false);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error!.Value.Message });
});

app.MapFallbackToFile("index.html");
app.Run();
