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
    void AddNewValue(double newValue, byte channel);
  }

  interface IOscillographContextSetter
  {
    IOscillographContext OscillographContext { set; }
  }

  class Oscillograph : WindowsFormsHost, IOscillographContext
  {
    const int CapacityDefault = 1000;
    const byte ChannelDefault = 0;
    const byte VisualizationOfEachPointFromDefault = 1;
    
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

    #region Зависимое свойство Channel.

    public static readonly DependencyProperty ChannelProperty =
      DependencyProperty.Register("Channel",
      typeof(byte),
      typeof(Oscillograph), new FrameworkPropertyMetadata(ChannelDefault, ChannelPropertyChangedCallback));

    private static void ChannelPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Oscillograph obj = d as Oscillograph;
      if (obj != null && e.NewValue is byte)
      {
        obj.ChannelPropertyChanged((byte)e.NewValue);
      }
    }

    /// <summary>
    /// Номер канала для вывода.
    /// </summary>
    public byte Channel
    {
      get { return (byte)GetValue(ChannelProperty); }
      set { SetValue(ChannelProperty, value); }
    }

    #endregion    

    #region Зависимое свойство VisualizationOfEachPointFrom.

    public static readonly DependencyProperty VisualizationOfEachPointFromProperty =
      DependencyProperty.Register("VisualizationOfEachPointFrom",
      typeof(byte),
      typeof(Oscillograph), new FrameworkPropertyMetadata(VisualizationOfEachPointFromDefault, VisualizationOfEachPointFromPropertyChangedCallback));

    private static void VisualizationOfEachPointFromPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Oscillograph obj = d as Oscillograph;
      if (obj != null && e.NewValue is byte)
      {
        obj.VisualizationOfEachPointFromPropertyChanged((byte)e.NewValue);
      }
    }

    /// <summary>
    /// Визуализация каждой точки из указанного числа. Остальные точки будут пропущены.
    /// Применяется для ускорения отрисовки графика.
    /// </summary>
    public byte VisualizationOfEachPointFrom
    {
      get { return (byte)GetValue(VisualizationOfEachPointFromProperty); }
      set { SetValue(VisualizationOfEachPointFromProperty, value); }
    }

    #endregion

    SynchronizationContext synchronizationContext;

    Chart chart;
    ChartArea chartArea;
    Series series;
    Series cursorSeries;

    /// <summary>
    /// Текущая точка для отображения.
    /// </summary>
    volatile int cursor;
    /// <summary>
    /// Номер канала для визуализации. Дублирует значение свойства Channel.
    /// </summary>
    volatile byte internalChannel;
    /// <summary>
    /// Отрисовка только указанной точки. Дублирует значение свойства VisualizationOfEachPointFrom.
    /// </summary>
    volatile byte internalVisualizationOfEachPointFrom;
    /// <summary>
    /// Счетчик для отображения точек указанных в VisualizationOfEachPointFrom.
    /// </summary>
    volatile int pointsCounter;

    public Oscillograph()
    {
      InitializeChart();

      cursor = 0;
      internalChannel = ChannelDefault;
      internalVisualizationOfEachPointFrom = VisualizationOfEachPointFromDefault;
      pointsCounter = 0;
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

    private void ChannelPropertyChanged(byte channel)
    {
      // Запомним в volatile переменной номер канала,
      // читать будем в AddNewValue в другом потоке, не в UI.
      internalChannel = channel;
    }

    private void VisualizationOfEachPointFromPropertyChanged(byte visualizationOfEachPointFrom)
    {
      internalVisualizationOfEachPointFrom = visualizationOfEachPointFrom;
    }

    #region IOscillographContext

    public void AddNewValue(double newValue, byte channel)
    {
      if (internalChannel == channel)
      {
        if (pointsCounter++ % internalVisualizationOfEachPointFrom == 0)
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
      }      
    }

    #endregion
  }
}
