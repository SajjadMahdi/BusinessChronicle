using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.DependencyInjection;
using BusinessChronicle.Samples.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.Samples.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        ServiceCollection services = new();
        services.AddBusinessChronicle(static options => options.Entity.AllowRollback = true);
        services.AddPropertyUnitCatalog();
        ServiceProvider provider = services.BuildServiceProvider();

        PropertyPortfolioForm form = new(
            provider.GetRequiredService<PropertyUnitCatalog>(),
            provider.GetRequiredService<IChronicleStore>());

        Application.Run(form);
    }
}
