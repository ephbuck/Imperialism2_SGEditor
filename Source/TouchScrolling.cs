using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Imperialism_2_SGEditor
{
  public class TouchScrolling : DependencyObject
  {
    public static bool GetIsEnabled(DependencyObject obj) { return (bool)obj.GetValue(IsEnabledProperty); }
    public static void SetIsEnabled(DependencyObject obj, bool value) { obj.SetValue(IsEnabledProperty, value); }
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(TouchScrolling), new UIPropertyMetadata(false, IsEnabledChanged));
    private static readonly DependencyProperty PanProperty = DependencyProperty.RegisterAttached(
        "Pan", typeof(PanState), typeof(TouchScrolling));

    private static void IsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      var viewer = obj as ScrollViewer;
      if (viewer == null) return;
      if ((bool)e.NewValue)
      {
        viewer.PreviewMouseDown += MouseDown;
        viewer.PreviewMouseMove += MouseMove;
        viewer.PreviewMouseUp += MouseUp;
        viewer.LostMouseCapture += LostCapture;
        viewer.Unloaded += Unloaded;
      }
      else
      {
        EndPan(viewer);
        viewer.PreviewMouseDown -= MouseDown;
        viewer.PreviewMouseMove -= MouseMove;
        viewer.PreviewMouseUp -= MouseUp;
        viewer.LostMouseCapture -= LostCapture;
        viewer.Unloaded -= Unloaded;
      }
    }

    private static void MouseDown(object sender, MouseButtonEventArgs e)
    {
      var viewer = (ScrollViewer)sender;
      if (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Right) return;
      if (viewer.GetValue(PanProperty) != null) return;
      // Scrollbar thumbs and buttons retain their normal input handling.
      var element = e.OriginalSource as DependencyObject;
      while (element != null && element != viewer)
      {
        if (element is ScrollBar || element is ButtonBase) return;
        var content = element as FrameworkContentElement;
        element = content != null ? content.Parent : VisualTreeHelper.GetParent(element);
      }
      viewer.SetValue(PanProperty, new PanState {
        Button = e.ChangedButton, Start = e.GetPosition(viewer),
        Horizontal = viewer.HorizontalOffset, Vertical = viewer.VerticalOffset,
        PreviousCursor = viewer.Cursor
      });
      // A press without dragging must still allow normal tile selection.
    }

    private static void MouseMove(object sender, MouseEventArgs e)
    {
      var viewer = (ScrollViewer)sender;
      var state = viewer.GetValue(PanProperty) as PanState;
      if (state == null) return;
      var pressed = state.Button == MouseButton.Left ? e.LeftButton : e.RightButton;
      if (pressed != MouseButtonState.Pressed) { EndPan(viewer); return; }
      Point position = e.GetPosition(viewer);
      double dx = position.X - state.Start.X, dy = position.Y - state.Start.Y;
      if (!state.Dragging)
      {
        if (Math.Abs(dx) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(dy) < SystemParameters.MinimumVerticalDragDistance) return;
        if (!viewer.CaptureMouse()) { EndPan(viewer); return; }
        state.Dragging = true;
        viewer.SetCurrentValue(FrameworkElement.CursorProperty, Cursors.Hand);
      }
      viewer.ScrollToHorizontalOffset(state.Horizontal - dx);
      viewer.ScrollToVerticalOffset(state.Vertical - dy);
      e.Handled = true;
    }

    private static void MouseUp(object sender, MouseButtonEventArgs e)
    {
      var viewer = (ScrollViewer)sender;
      var state = viewer.GetValue(PanProperty) as PanState;
      if (state == null || state.Button != e.ChangedButton) return;
      // Consume drag releases before they reach selection or ownership painting.
      if (state.Dragging) e.Handled = true;
      EndPan(viewer);
    }

    private static void LostCapture(object sender, MouseEventArgs e)
    {
      var viewer = (ScrollViewer)sender;
      if (!viewer.IsMouseCaptured) EndPan(viewer);
    }
    private static void Unloaded(object sender, RoutedEventArgs e) { EndPan((ScrollViewer)sender); }
    private static void EndPan(ScrollViewer viewer)
    {
      var state = viewer.GetValue(PanProperty) as PanState;
      viewer.ClearValue(PanProperty);
      if (state != null && state.Dragging) viewer.SetCurrentValue(FrameworkElement.CursorProperty, state.PreviousCursor);
      if (viewer.IsMouseCaptured) viewer.ReleaseMouseCapture();
    }
    private sealed class PanState
    {
      public MouseButton Button;
      public Point Start;
      public double Horizontal, Vertical;
      public bool Dragging;
      public Cursor PreviousCursor;
    }
  }
}