using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class ReportSettingsView : UserControl
{
    private string? _draggedPartyType;

    public ReportSettingsView(ReportSettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void HeaderGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var border = FindParentBorder(e.OriginalSource as DependencyObject);
        if (border?.Tag is string partyType)
        {
            _draggedPartyType = partyType;
            DragDrop.DoDragDrop(border, partyType, DragDropEffects.Move);
        }
    }

    private void HeaderGrid_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void HeaderGrid_Drop(object sender, DragEventArgs e)
    {
        if (_draggedPartyType == null) return;

        var targetBorder = FindParentBorder(e.OriginalSource as DependencyObject);
        if (targetBorder?.Tag is not string targetPartyType || targetPartyType == _draggedPartyType)
            return;

        var sourceIndex = Grid.GetColumn(FindBorderByTag(_draggedPartyType));
        var targetIndex = Grid.GetColumn(targetBorder);

        if (sourceIndex == targetIndex) return;

        var headerGrid = (Grid)sender;
        var sourceChild = headerGrid.Children[sourceIndex];
        var targetChild = headerGrid.Children[targetIndex];

        Grid.SetColumn(sourceChild, targetIndex);
        Grid.SetColumn(targetChild, sourceIndex);

        // Reorder children in visual tree to match new column positions
        headerGrid.Children.RemoveAt(sourceIndex);
        headerGrid.Children.Insert(targetIndex, sourceChild);

        var parties = headerGrid.Children
            .Cast<FrameworkElement>()
            .Select(c => c.Tag?.ToString())
            .Where(t => t != null);

        if (DataContext is ReportSettingsViewModel vm)
            vm.HeaderOrder = string.Join(",", parties);

        _draggedPartyType = null;
    }

    private static Border? FindParentBorder(DependencyObject? child)
    {
        while (child != null)
        {
            if (child is Border b && b.Tag is string)
                return b;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private Border? FindBorderByTag(string tag)
    {
        foreach (var child in HeaderGrid.Children)
        {
            if (child is Border b && b.Tag?.ToString() == tag)
                return b;
        }
        return null;
    }
}
