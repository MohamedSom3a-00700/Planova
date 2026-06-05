using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Planova.UI.Controls;

public class ActivityGanttCanvas : Canvas
{
    public static readonly DependencyProperty ActivitiesProperty =
        DependencyProperty.Register(nameof(Activities), typeof(object), typeof(ActivityGanttCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RelationshipsProperty =
        DependencyProperty.Register(nameof(Relationships), typeof(object), typeof(ActivityGanttCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ViewStartProperty =
        DependencyProperty.Register(nameof(ViewStart), typeof(DateTime), typeof(ActivityGanttCanvas),
            new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ViewEndProperty =
        DependencyProperty.Register(nameof(ViewEnd), typeof(DateTime), typeof(ActivityGanttCanvas),
            new FrameworkPropertyMetadata(DateTime.Today.AddDays(30), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ZoomLevelProperty =
        DependencyProperty.Register(nameof(ZoomLevel), typeof(string), typeof(ActivityGanttCanvas),
            new FrameworkPropertyMetadata("Week", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TimeScaleProperty =
        DependencyProperty.Register(nameof(TimeScale), typeof(double), typeof(ActivityGanttCanvas),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public object? Activities
    {
        get => GetValue(ActivitiesProperty);
        set => SetValue(ActivitiesProperty, value);
    }

    public object? Relationships
    {
        get => GetValue(RelationshipsProperty);
        set => SetValue(RelationshipsProperty, value);
    }

    public DateTime ViewStart
    {
        get => (DateTime)GetValue(ViewStartProperty);
        set => SetValue(ViewStartProperty, value);
    }

    public DateTime ViewEnd
    {
        get => (DateTime)GetValue(ViewEndProperty);
        set => SetValue(ViewEndProperty, value);
    }

    public string ZoomLevel
    {
        get => (string)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public double TimeScale
    {
        get => (double)GetValue(TimeScaleProperty);
        set => SetValue(TimeScaleProperty, value);
    }

    private readonly List<Visual> _visuals = [];

    protected override int VisualChildrenCount => _visuals.Count;

    protected override Visual GetVisualChild(int index) => _visuals[index];

    public void AddVisual(Visual visual)
    {
        _visuals.Add(visual);
        AddVisualChild(visual);
        AddLogicalChild(visual);
    }

    public void RemoveVisual(Visual visual)
    {
        _visuals.Remove(visual);
        RemoveVisualChild(visual);
        RemoveLogicalChild(visual);
    }

    public void ClearVisuals()
    {
        foreach (var visual in _visuals)
        {
            RemoveVisualChild(visual);
            RemoveLogicalChild(visual);
        }
        _visuals.Clear();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        ClearVisuals();

        var totalDays = (ViewEnd - ViewStart).Days;
        if (totalDays <= 0) return;

        var drawVisual = new DrawingVisual();
        using (var ctx = drawVisual.RenderOpen())
        {
            var widthPerDay = TimeScale;
            var totalWidth = totalDays * widthPerDay;
            var height = ActualHeight;

            for (int i = 0; i <= totalDays; i++)
            {
                var x = i * widthPerDay;
                ctx.DrawLine(new Pen(Brushes.LightGray, 0.5),
                    new Point(x, 0), new Point(x, height));
            }

            ctx.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, totalWidth, height));
        }
        AddVisual(drawVisual);
    }
}
