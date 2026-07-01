using System.CommandLine;
using BusinessChronicle.Cli.Commands;

return await BuildRootCommand().InvokeAsync(args);

static RootCommand BuildRootCommand()
{
    RootCommand root = new("BusinessChronicle command-line tools for audit and versioning.");
    root.AddCommand(DoctorCommand.Create());
    root.AddCommand(ListCommand.Create());
    root.AddCommand(HistoryCommand.Create());
    root.AddCommand(CompareCommand.Create());
    root.AddCommand(RollbackCommand.Create());
    root.AddCommand(ExportCommand.Create());
    root.AddCommand(InfoCommand.Create());
    return root;
}
