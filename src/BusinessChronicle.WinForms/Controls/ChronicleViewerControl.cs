namespace BusinessChronicle.WinForms.Controls;

/// <summary>
/// Composite chronicle viewer control.
/// </summary>
public sealed class ChronicleViewerControl : UserControl
{
    private readonly ChronicleTimelineControl _timeline = new();
    private readonly RevisionViewerControl _revisionViewer = new();
    private readonly DiffViewerControl _diffViewer = new();

    public ChronicleViewerControl()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 220,
        };

        split.Panel1.Controls.Add(_timeline);
        var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.Controls.Add(_revisionViewer, 0, 0);
        right.Controls.Add(_diffViewer, 0, 1);
        split.Panel2.Controls.Add(right);
        Controls.Add(split);
        Dock = DockStyle.Fill;
    }

    public ChronicleTimelineControl Timeline => _timeline;

    public RevisionViewerControl RevisionViewer => _revisionViewer;

    public DiffViewerControl DiffViewer => _diffViewer;
}
