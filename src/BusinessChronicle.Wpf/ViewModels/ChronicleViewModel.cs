using System.Collections.ObjectModel;
using System.Windows;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Wpf.ViewModels;

/// <summary>
/// MVVM view model for chronicle presentation.
/// </summary>
public sealed class ChronicleViewModel : System.ComponentModel.INotifyPropertyChanged
{
    private Revision? _selectedRevision;

    public ObservableCollection<TimelineEntry> TimelineEntries { get; } = [];

    public ObservableCollection<ChangeDescriptor> Changes { get; } = [];

    public Revision? SelectedRevision
    {
        get => _selectedRevision;
        set
        {
            _selectedRevision = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(SelectedRevision)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}
