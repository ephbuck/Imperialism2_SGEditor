using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Imperialism_2_SGEditor
{
  public partial class MainWindow : Window
  {
    public MainWindowVM VM { get; set; }


    public MainWindow()
    {
      InitializeComponent();
      VM = new MainWindowVM();
      DataContext = VM;
      PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private void UniformGrid_Loaded(object sender, RoutedEventArgs e)
    {
      //VM.LoadSlot0();
    }

    private bool _isSelectingField = false;

    private void MapFieldItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      //Title = DB.NotifyCounter.ToString();
      //DB.NotifyCounter = 0;
      //if (VM.SelectedMapField != null)
      //  VM.SelectedMapField.Update();
      Border border = (Border)sender;
      MapField field = (MapField)border.Tag;

      if (AutoBtnChkBx.IsChecked.Value && AutoBtnList.SelectedItem != null)
      {
        Country country = (Country)AutoBtnList.SelectedItem;
        if (AutoFieldBtnChkBx.IsChecked.Value)
        {
          field.Owner = country.Number;
          VM.UpdateCityOwner(field.Idx, country.Number);
          field.Update();
        }
        else
        {
          foreach (var fieldTemp in VM.WorldFields)
          {
            if (fieldTemp.ProvinceId == field.ProvinceId && fieldTemp.IsSea == field.IsSea)
            {
              fieldTemp.Owner = country.Number;
              if (fieldTemp.IsSea == false) VM.UpdateCityOwner(fieldTemp.Idx, country.Number);
              fieldTemp.Update();
            }
          }
        }
      }
      else
      {
        _isSelectingField = true;
        try
        {
          if (VM.SelectedMapField != null) VM.SelectedMapField.IsSelected = false;
          VM.SelectedMapField = field;
          //VM.SelectedMapField.RawData[22] = 5;
          //VM.SelectedMapField.Update();

          VM.RowItem.Clear();
          VM.RowItem.Add(VM.SelectedMapField);

          if (FieldDetailsDataGrid.Columns.Count == 0)
          {
            int columnIndex = 0;
            foreach (var data in VM.SelectedMapField.RawData)
            {
              FieldDetailsDataGrid.Columns.Add(
                  new DataGridTextColumn
                  {
                    Header = DB.HEADERS[columnIndex],
                    MinWidth = 30,
                    Binding = new Binding(string.Format("RawData[{0}]", columnIndex))
                  });
              columnIndex++;
            }
          }
          //FieldDetailsDataGrid.ItemsSource = VM.RowItem;
          VM.SelectedMapField.IsSelected = true;
        }
        finally
        {
          _isSelectingField = false;
        }
      }
    }

    private void FieldDetailsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      if (VM.SelectedMapField == null || e.EditAction != DataGridEditAction.Commit) return;

      int colIndex = e.Column.DisplayIndex;
      Dispatcher.BeginInvoke(new Action(() =>
      {
        if (colIndex == DB.LAND_TYPE_GFX)
        {
          DB.UpdateTerrainAndTransitions(VM.SelectedMapField, VM.WorldFields);
          VM.RowItem.Clear();
          VM.RowItem.Add(VM.SelectedMapField);
        }
        else
        {
          VM.SelectedMapField.Update();
        }
      }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void VisualTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_isSelectingField || VM.SelectedMapField == null) return;

      DB.UpdateTerrainAndTransitions(VM.SelectedMapField, VM.WorldFields);
      VM.RowItem.Clear();
      VM.RowItem.Add(VM.SelectedMapField);
    }

    private void ResourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_isSelectingField || VM.SelectedMapField == null) return;

      VM.SelectedMapField.Update();
      VM.RowItem.Clear();
      VM.RowItem.Add(VM.SelectedMapField);
    }

    private bool _isAdjustingZoomOffset = false;

    public void ZoomTo(double newZoom, Point? centerPointInViewport = null)
    {
      if (newZoom < 0.2) newZoom = 0.2;
      if (newZoom > 4.0) newZoom = 4.0;
      newZoom = Math.Round(newZoom, 2);

      double oldZoom = VM.Zoom;
      if (Math.Abs(oldZoom - newZoom) < 0.001) return;

      if (MapScrollViewer == null || MapScrollViewer.ViewportWidth <= 0 || MapScrollViewer.ViewportHeight <= 0)
      {
        VM.Zoom = newZoom;
        return;
      }

      Point center = centerPointInViewport ?? new Point(
          MapScrollViewer.ViewportWidth / 2.0,
          MapScrollViewer.ViewportHeight / 2.0);

      double contentX = (MapScrollViewer.HorizontalOffset + center.X) / oldZoom;
      double contentY = (MapScrollViewer.VerticalOffset + center.Y) / oldZoom;

      _isAdjustingZoomOffset = true;
      try
      {
        VM.Zoom = newZoom;
        MapScrollViewer.UpdateLayout();

        double targetH = contentX * newZoom - center.X;
        double targetV = contentY * newZoom - center.Y;

        MapScrollViewer.ScrollToHorizontalOffset(targetH);
        MapScrollViewer.ScrollToVerticalOffset(targetV);
      }
      finally
      {
        _isAdjustingZoomOffset = false;
      }
    }

    private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      {
        e.Handled = true;

        double factor = e.Delta > 0 ? 1.15 : (1.0 / 1.15);
        double targetZoom = VM.Zoom * factor;

        if (Math.Abs(targetZoom - 1.0) < 0.05)
        {
          targetZoom = 1.0;
        }

        Point mousePos = e.GetPosition(MapScrollViewer);
        ZoomTo(targetZoom, mousePos);
      }
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      {
        if (e.Key == Key.OemPlus || e.Key == Key.Add)
        {
          ZoomTo(VM.Zoom * 1.25, null);
          e.Handled = true;
        }
        else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
        {
          ZoomTo(VM.Zoom / 1.25, null);
          e.Handled = true;
        }
        else if (e.Key == Key.D0 || e.Key == Key.NumPad0)
        {
          ZoomTo(1.0, null);
          e.Handled = true;
        }
      }
    }

    private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
    {
      ZoomTo(VM.Zoom / 1.25, null);
    }

    private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
    {
      ZoomTo(VM.Zoom * 1.25, null);
    }

    private void ZoomResetBtn_Click(object sender, RoutedEventArgs e)
    {
      ZoomTo(1.0, null);
    }

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (_isAdjustingZoomOffset) return;
      if (MapScrollViewer == null || VM == null) return;

      ZoomTo(e.NewValue, null);
    }
  }
}
