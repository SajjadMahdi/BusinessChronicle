using BusinessChronicle.AspNetCore.DependencyInjection;
using BusinessChronicle.AspNetCore.Endpoints;
using BusinessChronicle.Samples.Shared.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddBusinessChronicle(static options => options.Entity.AllowRollback = true);
builder.Services.AddPropertyUnitCatalog();

WebApplication app = builder.Build();

PropertyUnitCatalog catalog = app.Services.GetRequiredService<PropertyUnitCatalog>();
await catalog.EnsureSeededAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapBusinessChronicle();
app.Run();
