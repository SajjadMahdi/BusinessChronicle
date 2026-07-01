using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Wpf.Controls;

/// <summary>
/// Timeline presentation control.
/// </summary>
public partial class TimelineControl : UserControl
{
    public static readonly DependencyProperty TimelineEntriesProperty =
        DependencyProperty.Register(nameof(TimelineEntries), typeof(ObservableCollection<TimelineEntry>), typeof(TimelineControl), new PropertyMetadata(new ObservableCollection<TimelineEntry>()));

    public ObservableCollection<TimelineEntry> TimelineEntries
    {
        get => (ObservableCollection<TimelineEntry>)GetValue(TimelineEntriesProperty);
        set => SetValue(TimelineEntriesProperty, value);
    }

    public TimelineControl() => InitializeComponent();
}
