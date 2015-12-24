namespace EcgFftDemo
{
  partial class MainForm
  {
    /// <summary>
    /// Требуется переменная конструктора.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Освободить все используемые ресурсы.
    /// </summary>
    /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Код, автоматически созданный конструктором форм Windows

    /// <summary>
    /// Обязательный метод для поддержки конструктора - не изменяйте
    /// содержимое данного метода при помощи редактора кода.
    /// </summary>
    private void InitializeComponent()
    {
      this.label1 = new System.Windows.Forms.Label();
      this.txtFile = new System.Windows.Forms.TextBox();
      this.btnOpen = new System.Windows.Forms.Button();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCalc = new System.Windows.Forms.Button();
      this.numDepth = new System.Windows.Forms.NumericUpDown();
      this.numInterval = new System.Windows.Forms.NumericUpDown();
      this.numBegin = new System.Windows.Forms.NumericUpDown();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.tabPage5 = new System.Windows.Forms.TabPage();
      this.txtCode = new System.Windows.Forms.TextBox();
      this.chkOnlyFirst = new System.Windows.Forms.CheckBox();
      this.tabControl.SuspendLayout();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numDepth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numBegin)).BeginInit();
      this.tabPage5.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 10);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(39, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Файл:";
      // 
      // txtFile
      // 
      this.txtFile.Location = new System.Drawing.Point(48, 7);
      this.txtFile.Name = "txtFile";
      this.txtFile.ReadOnly = true;
      this.txtFile.Size = new System.Drawing.Size(576, 20);
      this.txtFile.TabIndex = 1;
      // 
      // btnOpen
      // 
      this.btnOpen.Location = new System.Drawing.Point(630, 5);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(75, 23);
      this.btnOpen.TabIndex = 2;
      this.btnOpen.Text = "Открыть";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
      // 
      // tabControl
      // 
      this.tabControl.Controls.Add(this.tabPage1);
      this.tabControl.Controls.Add(this.tabPage2);
      this.tabControl.Controls.Add(this.tabPage3);
      this.tabControl.Controls.Add(this.tabPage4);
      this.tabControl.Controls.Add(this.tabPage5);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.Location = new System.Drawing.Point(0, 62);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(912, 537);
      this.tabControl.TabIndex = 3;
      // 
      // tabPage1
      // 
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(904, 511);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "I Отведение";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabPage2
      // 
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(904, 511);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "II Отведение";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // tabPage3
      // 
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(904, 511);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "III Отведение";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // tabPage4
      // 
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(904, 511);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Остальные отведения";
      this.tabPage4.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.chkOnlyFirst);
      this.panel1.Controls.Add(this.btnCalc);
      this.panel1.Controls.Add(this.numDepth);
      this.panel1.Controls.Add(this.numInterval);
      this.panel1.Controls.Add(this.numBegin);
      this.panel1.Controls.Add(this.label4);
      this.panel1.Controls.Add(this.label3);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Controls.Add(this.btnOpen);
      this.panel1.Controls.Add(this.txtFile);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(912, 62);
      this.panel1.TabIndex = 4;
      // 
      // btnCalc
      // 
      this.btnCalc.Location = new System.Drawing.Point(630, 33);
      this.btnCalc.Name = "btnCalc";
      this.btnCalc.Size = new System.Drawing.Size(75, 23);
      this.btnCalc.TabIndex = 9;
      this.btnCalc.Text = "Расчет";
      this.btnCalc.UseVisualStyleBackColor = true;
      this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
      // 
      // numDepth
      // 
      this.numDepth.Location = new System.Drawing.Point(554, 33);
      this.numDepth.Name = "numDepth";
      this.numDepth.Size = new System.Drawing.Size(70, 20);
      this.numDepth.TabIndex = 8;
      // 
      // numInterval
      // 
      this.numInterval.Location = new System.Drawing.Point(321, 33);
      this.numInterval.Name = "numInterval";
      this.numInterval.Size = new System.Drawing.Size(70, 20);
      this.numInterval.TabIndex = 7;
      // 
      // numBegin
      // 
      this.numBegin.Location = new System.Drawing.Point(147, 33);
      this.numBegin.Name = "numBegin";
      this.numBegin.Size = new System.Drawing.Size(70, 20);
      this.numBegin.TabIndex = 6;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(411, 35);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(137, 13);
      this.label4.TabIndex = 5;
      this.label4.Text = "Глубина восстановления:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(234, 35);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(81, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Окно анализа:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(45, 35);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(96, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Начальная точка:";
      // 
      // tabPage5
      // 
      this.tabPage5.Controls.Add(this.txtCode);
      this.tabPage5.Location = new System.Drawing.Point(4, 22);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Size = new System.Drawing.Size(904, 511);
      this.tabPage5.TabIndex = 4;
      this.tabPage5.Text = "Спектр";
      this.tabPage5.UseVisualStyleBackColor = true;
      // 
      // txtCode
      // 
      this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtCode.Location = new System.Drawing.Point(0, 0);
      this.txtCode.Multiline = true;
      this.txtCode.Name = "txtCode";
      this.txtCode.ReadOnly = true;
      this.txtCode.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtCode.Size = new System.Drawing.Size(904, 511);
      this.txtCode.TabIndex = 0;
      // 
      // chkOnlyFirst
      // 
      this.chkOnlyFirst.AutoSize = true;
      this.chkOnlyFirst.Location = new System.Drawing.Point(711, 37);
      this.chkOnlyFirst.Name = "chkOnlyFirst";
      this.chkOnlyFirst.Size = new System.Drawing.Size(199, 17);
      this.chkOnlyFirst.TabIndex = 10;
      this.chkOnlyFirst.Text = "Расчет только первого отведения";
      this.chkOnlyFirst.UseVisualStyleBackColor = true;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(912, 599);
      this.Controls.Add(this.tabControl);
      this.Controls.Add(this.panel1);
      this.Name = "MainForm";
      this.Text = "EcgFourierDemo";
      this.tabControl.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numDepth)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numBegin)).EndInit();
      this.tabPage5.ResumeLayout(false);
      this.tabPage5.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtFile;
    private System.Windows.Forms.Button btnOpen;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.NumericUpDown numDepth;
    private System.Windows.Forms.NumericUpDown numInterval;
    private System.Windows.Forms.NumericUpDown numBegin;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnCalc;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.TabPage tabPage4;
    private System.Windows.Forms.TabPage tabPage5;
    private System.Windows.Forms.TextBox txtCode;
    private System.Windows.Forms.CheckBox chkOnlyFirst;
  }
}

