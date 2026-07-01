using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.WinForms.Controls;

/// <summary>
/// Diff viewer control for WinForms.
/// </summary>
public sealed partial class DiffViewerControl : DataGridView
{
    public DiffViewerControl()
    {
        Dock = DockStyle.Fill;
        ReadOnly = true;
        AutoGenerateColumns = true;
        AllowUserToAddRows = false;
    }

    public void BindChanges(IEnumerable<ChangeDescriptor> changes) => DataSource = changes.ToList();
}
