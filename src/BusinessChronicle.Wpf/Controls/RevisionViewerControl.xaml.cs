using System.Windows;
using System.Windows.Controls;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Wpf.Controls;

/// <summary>
/// Revision details control.
/// </summary>
public partial class RevisionViewerControl : UserControl
{
    public static readonly DependencyProperty RevisionProperty =
        DependencyProperty.Register(nameof(Revision), typeof(Revision), typeof(RevisionViewerControl));

    public Revision? Revision
    {
        get => (Revision?)GetValue(RevisionProperty);
        set => SetValue(RevisionProperty, value);
    }

    public RevisionViewerControl() => InitializeComponent();
}
