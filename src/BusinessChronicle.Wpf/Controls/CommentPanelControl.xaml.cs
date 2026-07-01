using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Wpf.Controls;

/// <summary>
/// Comment panel control.
/// </summary>
public partial class CommentPanelControl : UserControl
{
    public static readonly DependencyProperty CommentsProperty =
        DependencyProperty.Register(nameof(Comments), typeof(ObservableCollection<ChronicleComment>), typeof(CommentPanelControl), new PropertyMetadata(new ObservableCollection<ChronicleComment>()));

    public ObservableCollection<ChronicleComment> Comments
    {
        get => (ObservableCollection<ChronicleComment>)GetValue(CommentsProperty);
        set => SetValue(CommentsProperty, value);
    }

    public CommentPanelControl() => InitializeComponent();
}
