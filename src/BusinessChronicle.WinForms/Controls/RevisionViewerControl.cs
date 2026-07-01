using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.WinForms.Controls;

/// <summary>
/// Revision details control for WinForms.
/// </summary>
public sealed partial class RevisionViewerControl : UserControl
{
    private readonly Label _idLabel = new() { Dock = DockStyle.Top, Height = 24 };
    private readonly Label _metaLabel = new() { Dock = DockStyle.Fill };

    public RevisionViewerControl()
    {
        Controls.Add(_metaLabel);
        Controls.Add(_idLabel);
        Dock = DockStyle.Top;
        Height = 120;
    }

    public void BindRevision(Revision? revision)
    {
        if (revision is null)
        {
            _idLabel.Text = "No revision";
            _metaLabel.Text = string.Empty;
            return;
        }

        _idLabel.Text = revision.Id.Value;
        _metaLabel.Text = $"{revision.Type} | v{revision.Version?.Number} | {revision.CreatedAt:u}";
    }
}
