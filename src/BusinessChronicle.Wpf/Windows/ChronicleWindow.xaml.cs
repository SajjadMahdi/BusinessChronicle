using System.Windows;
using BusinessChronicle.Wpf.ViewModels;

namespace BusinessChronicle.Wpf.Windows;

/// <summary>
/// Standalone chronicle presentation window.
/// </summary>
public partial class ChronicleWindow : Window
{
    public ChronicleWindow()
    {
        InitializeComponent();
        DataContext = new ChronicleViewModel();
    }

    public ChronicleWindow(ChronicleViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
