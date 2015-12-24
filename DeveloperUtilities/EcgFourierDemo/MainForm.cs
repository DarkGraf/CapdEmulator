using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace EcgFftDemo
{
  public partial class MainForm : Form
  {
    Dictionary<int, Chart[]> dictCharts;
    EcgReader ecgReader;
    // Первый элемент - оригинальный сигнал, второй элемент - преобразованный сигнал.
    Tuple<double[], double[]>[] signals;
    List<Complex[]> spectors;

    public MainForm()
    {
      InitializeComponent();

      dictCharts = new Dictionary<int, Chart[]>();
      signals = new Tuple<double[], double[]>[9];
      spectors = new List<Complex[]>();

      TableLayoutPanel tableLayoutPanel;
      // Создаем для трех отведений.
      for (int t = 0; t < 3; t++)
      {
        tableLayoutPanel = new TableLayoutPanel();
        tabControl.TabPages[t].Controls.Add(tableLayoutPanel);
        tableLayoutPanel.Dock = DockStyle.Fill;        
        tableLayoutPanel.ColumnCount = 2;
        tableLayoutPanel.RowCount = 4;
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

        Chart[] charts = new Chart[6];
        for (int i = 0; i < 6; i++)
        {
          int row = i < 3 ? i / 2 : (i + 1) / 2;
          int col = i < 3 ? i % 2 : (i + 1) % 2;

          Chart chart = charts[i] = CreateChart();
          tableLayoutPanel.Controls.Add(chart);
          tableLayoutPanel.SetRow(chart, row);
          tableLayoutPanel.SetColumn(chart, col);
          if ((i + 1) % 3 == 0)
          {
            tableLayoutPanel.SetColumnSpan(chart, 2);
          }
        }
        dictCharts[t] = charts;
      }

      // Создаем для остальных отведений.
      tableLayoutPanel = new TableLayoutPanel();
      tabControl.TabPages[3].Controls.Add(tableLayoutPanel);
      tableLayoutPanel.Dock = DockStyle.Fill;
      tableLayoutPanel.ColumnCount = 1;
      tableLayoutPanel.RowCount = 9;
      tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      for (int i = 0; i < 9; i++)
      {
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / 9));
      }
      for (int i = 0; i < 9; i++)
      {
        Chart chart = CreateChart();
        tableLayoutPanel.Controls.Add(chart);
        tableLayoutPanel.SetRow(chart, i);
        tableLayoutPanel.SetColumn(chart, 0);
        dictCharts[3 + i] = new Chart[] { chart };

        chart.Titles.Add(new string[] { "AVR", "AVL", "AVF", "C1", "C2", "C3", "C4", "C5", "C6" }[i]);
        chart.Titles[0].Docking = Docking.Left; 
      }

      EnableButtons(false);
    }

    private void EnableButtons(bool enable)
    {
      numBegin.Enabled = numInterval.Enabled = numDepth.Enabled = btnCalc.Enabled = enable;
    }

    private Chart CreateChart()
    {
      Chart chart;
      ChartArea chartArea;
      Series series;

      chart = new Chart();

      chartArea = new ChartArea();
      chartArea.Name = "chartArea";
      chart.ChartAreas.Add(chartArea);

      series = new Series();
      series.ChartArea = "chartArea";
      series.ChartType = SeriesChartType.FastLine;
      chart.Series.Add(series);

      chart.Dock = DockStyle.Fill;
      return chart;
    }

    private void btnOpen_Click(object sender, EventArgs e)
    {
      OpenFileDialog dialog = new OpenFileDialog();
      dialog.Filter = "*.emul|*.emul";
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        EnableButtons(false);
        EmulFileReader fileReader = new EmulFileReader(dialog.FileName);
        fileReader.Read();
        ecgReader = new EcgReader(fileReader);
        if (ecgReader.Channels.Count == 9)
        {
          txtFile.Text = dialog.FileName;
          EnableButtons(true);

          numBegin.Maximum = ecgReader.Channels[0].Points.Count;
          numBegin.Value = 1300;
          numInterval.Maximum = ecgReader.Channels[0].Points.Count;
          numInterval.Value = 1200;
          numDepth.Maximum = ecgReader.Channels[0].Points.Count;
          numDepth.Value = 35;

          FillCharts();
        }
        else
        {
          MessageBox.Show("Файл эмуляции не содержит данных ЭКГ.");
        }
      }
    }

    private void btnCalc_Click(object sender, EventArgs e)
    {
      FillCharts();
    }

    private void Calc()
    {
      int begin = (int)numBegin.Value;
      int interval = (int)numInterval.Value;
      int depth = (int)numDepth.Value;

      spectors.Clear();
      for (byte i = 0; i < 9; i++)
      {
        // Если стоит признак расчета только первого отведения,
        // запишим для других пустые массивы.
        if (chkOnlyFirst.Checked && i != 6 && i != 7)
        {
          double[] fake = Enumerable.Range(0, interval).Select(v => (double)0).ToArray();
          signals[i] = new Tuple<double[], double[]>(fake, fake);
          continue;
        }

        double[] original = ecgReader.Channels[i].Points.Skip(begin).Take(interval).ToArray();
        // DFT.
        Complex[] raw = original.Select(v => new Complex(v, 0)).ToArray();

        // Восстановление сигнала обратным преобразованием Фурье.
        Complex[] spector = DFT.FourierTransform(raw).Take(depth).ToArray();
        Complex[] complex = DFT.InverseFourierTransform(spector, interval);
        double[] transform = complex.Select(c => c.Real).ToArray();
        signals[i] = new Tuple<double[], double[]>(original, transform);
        spectors.Add(spector);
      }
    }

    private void FillCharts()
    {
      // Если стоит признак расчета только первого отведения,
      // скроем на других вкладках элементы управления.
      for (int i = 1; i < tabControl.TabCount; i++)
        tabControl.TabPages[i].Controls[0].Visible = !chkOnlyFirst.Checked;

      Calc();

      // Первое отведение.
      FillChart(signals[7].Item1, signals[6].Item1, dictCharts[0], ecgReader.Multipliers[1], ecgReader.Multipliers[0]);
      FillDftChart(signals[7].Item2, signals[6].Item2, dictCharts[0], ecgReader.Multipliers[1], ecgReader.Multipliers[0]);
      // Второе отведение.
      FillChart(signals[8].Item1, signals[6].Item1, dictCharts[1], ecgReader.Multipliers[2], ecgReader.Multipliers[0]);
      FillDftChart(signals[8].Item2, signals[6].Item2, dictCharts[1], ecgReader.Multipliers[2], ecgReader.Multipliers[0]);
      // Третье отведение.
      FillChart(signals[8].Item1, signals[7].Item1, dictCharts[2], ecgReader.Multipliers[2], ecgReader.Multipliers[1]);
      FillDftChart(signals[8].Item2, signals[7].Item2, dictCharts[2], ecgReader.Multipliers[2], ecgReader.Multipliers[1]);

      FillOtherCharts();

      CreateCode();
    }

    private void FillChart(double[] firstOtv, double[] secondOtv, Chart[] charts, double firstMultiplier, double secondMultiplier)
    {
      foreach (Chart chart in charts.Take(3))
      {
        chart.Series[0].Points.Clear();
      }

      foreach (int i in firstOtv)
      {
        charts[0].Series[0].Points.AddY(i);
      }

      foreach (int i in secondOtv)
      {
        charts[1].Series[0].Points.AddY(i);
      }

      double[] result = new double[firstOtv.Length];
      for (int i = 0; i < result.Length; i++)
      {
        result[i] = firstOtv[i] * firstMultiplier - secondOtv[i] * secondMultiplier;
        charts[2].Series[0].Points.AddY(result[i]);
      }
    }

    private void FillDftChart(double[] firstOtv, double[] secondOtv, Chart[] charts, double firstMultiplier, double secondMultiplier)
    {
      foreach (Chart chart in charts.Skip(3))
      {
        chart.Series[0].Points.Clear();
      }

      foreach (int i in firstOtv)
      {
        charts[3].Series[0].Points.AddY(i);
      }

      foreach (int i in secondOtv)
      {
        charts[4].Series[0].Points.AddY(i);
      }

      double[] result = new double[firstOtv.Length];
      for (int i = 0; i < result.Length; i++)
      {
        result[i] = firstOtv[i] * firstMultiplier - secondOtv[i] * secondMultiplier;
        charts[5].Series[0].Points.AddY(result[i]);
      }
      RecalculateAxesScale(charts[5], result);
    }

    private void FillOtherCharts()
    {
      List<double[]> list = new List<double[]>();
      for (int i = 0; i < 9; i++)
      {
        double multiplier;
        if (i >= 6 && i <= 8)
          multiplier = ecgReader.Multipliers[i - 6]; // 0-R, 1-L, 2-F.
        else
          multiplier = ecgReader.Multipliers[3 + i]; // C1...C6.

        double[] signal = signals[i].Item2.Select(v => v * multiplier).ToArray();
        list.Add(signal);
      }

      double[] points = new double[list[0].Length];

      for (int i = 3; i < 12; i++)
      {
        switch (i)
        {
          case 3: // AVR = R - (F + L) / 2
            for (int j = 0; j < points.Length; j++)
              points[j] = list[6][j] - (list[8][j] + list[7][j]) / 2;
            break;
          case 4: // AVL = L - (R + F) / 2
            for (int j = 0; j < points.Length; j++)
              points[j] = list[7][j] - (list[6][j] + list[8][j]) / 2;
            break;
          case 5: // AVF = L - (R + L) / 2
            for (int j = 0; j < points.Length; j++)
              points[j] = list[7][j] - (list[6][j] + list[7][j]) / 2;
            break;
          case 6:
          case 7:
          case 8:
          case 9:
          case 10:
          case 11: // Ci - (R + L + F) / 2
            for (int j = 0; j < points.Length; j++)
              points[j] = list[i - 6][j] - (list[6][j] + list[7][j] + list[8][j]) / 3;
            break;
        }

        dictCharts[i][0].Series[0].Points.Clear();
        for (int j = 0; j < points.Length; j++)
        {
          dictCharts[i][0].Series[0].Points.AddY(points[j]);
        }
        RecalculateAxesScale(dictCharts[i][0], points);
      }
    }

    private void RecalculateAxesScale(Chart chart, double[] points)
    {
      chart.ChartAreas[0].AxisY.Maximum = Math.Round(points.Max() + 0.1, 1);
      chart.ChartAreas[0].AxisY.Minimum = Math.Round(points.Min() - 0.1, 1);
    }

    private void CreateCode()
    {
      txtCode.Clear();

      StringBuilder sb = new StringBuilder();
      sb.Append(string.Format("int ecgPeriod = {0};", numInterval.Value));
      sb.Append(Environment.NewLine);
      sb.Append("Complex[][] ecgSpectors = new Complex[][]");
      sb.Append(Environment.NewLine);
      sb.Append("{");
      sb.Append(Environment.NewLine);
      foreach (Complex[] s in spectors)
      {
        sb.Append("  new Complex[]");
        sb.Append(Environment.NewLine);
        sb.Append("  {");
        sb.Append(Environment.NewLine);
        for (int i = 0; i < s.Length; i++)
        {
          sb.Append(string.Format(CultureInfo.InvariantCulture, "    new Complex({0}, {1})", s[i].Real, s[i].Imaginary));
          if (i != s.Length - 1)
          {
            sb.Append(",");
          }
          sb.Append(Environment.NewLine);
        }
        sb.Append("  }");
        if (spectors.Last() != s)
          sb.Append(",");
        sb.Append(Environment.NewLine);
      }
      sb.Append("};");
      txtCode.Text += sb.ToString();
    }
  }
}