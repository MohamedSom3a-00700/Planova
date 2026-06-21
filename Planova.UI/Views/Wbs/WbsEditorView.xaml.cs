using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Planova.UI.ViewModels.Wbs;
using Planova.Wbs.Domain.Entities;

namespace Planova.UI.Views.Wbs;

public partial class WbsEditorView : UserControl
{
    private Point _dragStartPoint;

    public WbsEditorView(WbsEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _dragStartPoint.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - _dragStartPoint.Y) <= SystemParameters.MinimumVerticalDragDistance)
            return;

        if (sender is not ListView listView) return;
        var item = listView.SelectedItem as WbsItem;
        if (item == null) return;

        DragDrop.DoDragDrop(listView, item, DragDropEffects.Move);
    }

    private void ListView_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(WbsItem)) is not WbsItem draggedItem) return;
        if (DataContext is not WbsEditorViewModel vm) return;

        vm.DragDropItemId = draggedItem.Id;
        vm.HandleDropCommand.Execute(null);
    }
}
