using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CapdxxTester.Views.Controls
{
  interface IOscillographContext
  {
    void AddNewValue(double newValue);
  }

  interface IOscillographContextSetter
  {
    IOscillographContext OscillographContext { get; set; }
  }

  class Oscillograph : WindowsFormsHost, IOscillographContext
  {
    const int CapacityDefault = 1000;
    
    #region Зависимое свойство Capacity.

    public static readonly DependencyProperty CapacityProperty =
      DependencyProperty.Register("Capacity",
      typeof(int),
      typeof(Oscillograph), new FrameworkPropertyMetadata(CapacityDefault, CapacityPropertyChangedCallback));

    private static void CapacityPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Oscillograph obj = d as Oscillograph;
      if (obj != null && e.NewValue is int)
      {
        obj.CapacityPropertyChanged((int)e.NewValue);
      }
    }

    /// <summary>
    /// Количество точек по абсциссе.
    /// </summary>
    public int Capacity
    {
      get { return (int)GetValue(CapacityProperty); }
      set { SetValue(CapacityProperty, value); }
    }

    #endregion

    #region Зависимое свойство OscillographContextSetter.

    public static readonly DependencyProperty OscillographContextSetterProperty =
      DependencyProperty.Register("OscillographContextSetter",
      typeof(IOscillographContextSetter),
      typeof(Oscillograph), new FrameworkPropertyMetadata(OscillographContextSetterPropertyChangedCallback));

    private static void OscillographContextSetterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Oscillograph obj = d as Oscillograph;
      if (obj != null && e.NewValue is IOscillographContextSetter)
      {
        obj.OscillographContextSetterPropertyChanged(e.NewValue as IOscillographContextSetter);
      }
    }

    /// <summary>
    /// Очередное новое значение для помещения в осцилограф.
    /// </summary>
    public IOscillographContextSetter OscillographContextSetter
    {
      get { return (IOscillographContextSetter)GetValue(OscillographContextSetterProperty); }
      set { SetValue(OscillographContextSetterProperty, value); }
    }

    #endregion

    SynchronizationContext synchronizationContext;

    Chart chart;
    ChartArea chartArea;
    Series series;
    Series cursorSeries;

    volatile int cursor = 0;

    public Oscillograph()
    {
      InitializeChart();
    }

    private void InitializeChart()
    {
      chart = new Chart();
      chart.BackColor = System.Drawing.Color.Black;

      chartArea = new ChartArea();
      chartArea.BackColor = System.Drawing.Color.Black;
      chartArea.Name = "chartArea";
      chartArea.AxisX.MajorGrid.Enabled = false;
      chartArea.AxisX.MinorGrid.Enabled = false;
      chartArea.AxisY.MajorGrid.Enabled = false;
      chartArea.AxisY.MinorGrid.Enabled = false;
      chartArea.AxisX.Enabled = AxisEnabled.False;
      chartArea.AxisY.Enabled = AxisEnabled.False;
      chart.ChartAreas.Add(chartArea);

      series = new Series();
      series.ChartArea = "chartArea";
      series.ChartType = SeriesChartType.FastLine;
      series.Color = System.Drawing.Color.Red;
      chart.Series.Add(series);

      cursorSeries = new Series();
      cursorSeries.ChartArea = "chartArea";
      cursorSeries.ChartType = SeriesChartType.FastLine;
      cursorSeries.Color = System.Drawing.Color.Red;
      chart.Series.Add(cursorSeries);

      chart.Size = new System.Drawing.Size(900, 400);
      chart.Location = new System.Drawing.Point(12, 12);
      Child = chart;

      for (int i = 0; i < Capacity; i++)
      {
        series.Points.AddXY(i, 0);
      }

      cursorSeries.Points.AddXY(0, 0);
      cursorSeries.Points.AddXY(0, 0);

      synchronizationContext = SynchronizationContext.Current;
    }

    private void CapacityPropertyChanged(int capacity)
    {
      InitializeChart();
    }

    private void OscillographContextSetterPropertyChanged(IOscillographContextSetter setter)
    {
      if (setter != null)
        setter.OscillographContext = this;
    }

    #region IOscillographContext
    
    public void AddNewValue(double newValue)
    {
      synchronizationContext.Post(delegate
      {
        series.Points[cursor] = new DataPoint(cursor, newValue);

        if (newValue < chartArea.AxisY.Minimum || newValue > chartArea.AxisY.Maximum)
          chartArea.RecalculateAxesScale();
        cursorSeries.Points[0] = new DataPoint(cursor, chartArea.AxisY.Minimum * 0.99);
        cursorSeries.Points[1] = new DataPoint(cursor, chartArea.AxisY.Maximum * 0.99);

        cursor++;
        cursor %= Capacity;
      }, null);
    }

    #endregion
  }
}
