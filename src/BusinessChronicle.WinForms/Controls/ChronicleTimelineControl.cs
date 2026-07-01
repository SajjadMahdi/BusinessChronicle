using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.WinForms.Controls;

/// <summary>
/// Timeline list control for WinForms.
/// </summary>
public sealed partial class ChronicleTimelineControl : ListView
{
    public ChronicleTimelineControl()
    {
        View = View.Details;
        FullRowSelect = true;
        Columns.Add("Seq", 40);
        Columns.Add("Type", 80);
        Columns.Add("When", 160);
        Columns.Add("Revision", 200);
    }

    public void BindEntries(IEnumerable<TimelineEntry> entries)
    {
        Items.Clear();
        foreach (TimelineEntry entry in entries)
        {
            ListViewItem item = new(entry.Entry.RevisionId.Value)
            {
                SubItems =
                {
                    entry.Sequence.ToString(),
                    entry.Entry.RevisionType.ToString(),
                    entry.Entry.OccurredAt.ToString("u"),
                },
            };
            Items.Add(item);
        }
    }
}
