using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Wpf.Controls;

/// <summary>
/// Diff presentation control.
/// </summary>
public partial class DiffViewerControl : UserControl
{
    public static readonly DependencyProperty ChangesProperty =
        DependencyProperty.Register(nameof(Changes), typeof(ObservableCollection<ChangeDescriptor>), typeof(DiffViewerControl), new PropertyMetadata(new ObservableCollection<ChangeDescriptor>()));

    public ObservableCollection<ChangeDescriptor> Changes
    {
        get => (ObservableCollection<ChangeDescriptor>)GetValue(ChangesProperty);
        set => SetValue(ChangesProperty, value);
    }

    public DiffViewerControl() => InitializeComponent();
}
