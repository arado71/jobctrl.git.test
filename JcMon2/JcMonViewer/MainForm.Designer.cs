namespace JcMonViewer
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lbCaptures = new System.Windows.Forms.ListBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.pbScreenshot = new System.Windows.Forms.PictureBox();
			this.label14 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.lblName = new System.Windows.Forms.Label();
			this.lblClassName = new System.Windows.Forms.Label();
			this.lblText = new System.Windows.Forms.Label();
			this.lblValue = new System.Windows.Forms.Label();
			this.lblControlType = new System.Windows.Forms.Label();
			this.lblHelpText = new System.Windows.Forms.Label();
			this.lblSelection = new System.Windows.Forms.Label();
			this.lblAutomationId = new System.Windows.Forms.Label();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblWinClassName = new System.Windows.Forms.Label();
			this.lblProcessName = new System.Windows.Forms.Label();
			this.lblNote = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.tvControl = new System.Windows.Forms.TreeView();
			this.tvWindow = new System.Windows.Forms.TreeView();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbScreenshot)).BeginInit();
			this.tableLayoutPanel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
			this.tableLayoutPanel1.Controls.Add(this.lbCaptures, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1118, 741);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lbCaptures
			// 
			this.lbCaptures.DisplayMember = "DisplayCaption";
			this.lbCaptures.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbCaptures.FormattingEnabled = true;
			this.lbCaptures.Location = new System.Drawing.Point(3, 3);
			this.lbCaptures.Name = "lbCaptures";
			this.lbCaptures.Size = new System.Drawing.Size(194, 735);
			this.lbCaptures.TabIndex = 3;
			this.lbCaptures.SelectedValueChanged += new System.EventHandler(this.HandleCaptureSelectionChanged);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.label5, 0, 8);
			this.tableLayoutPanel2.Controls.Add(this.label6, 0, 13);
			this.tableLayoutPanel2.Controls.Add(this.label3, 0, 12);
			this.tableLayoutPanel2.Controls.Add(this.pbScreenshot, 0, 14);
			this.tableLayoutPanel2.Controls.Add(this.label14, 0, 7);
			this.tableLayoutPanel2.Controls.Add(this.label4, 0, 11);
			this.tableLayoutPanel2.Controls.Add(this.label13, 0, 6);
			this.tableLayoutPanel2.Controls.Add(this.label2, 0, 10);
			this.tableLayoutPanel2.Controls.Add(this.label12, 0, 5);
			this.tableLayoutPanel2.Controls.Add(this.label11, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.label10, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.label9, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.label15, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.label16, 0, 9);
			this.tableLayoutPanel2.Controls.Add(this.lblName, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.lblClassName, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.lblText, 1, 3);
			this.tableLayoutPanel2.Controls.Add(this.lblValue, 1, 4);
			this.tableLayoutPanel2.Controls.Add(this.lblControlType, 1, 5);
			this.tableLayoutPanel2.Controls.Add(this.lblHelpText, 1, 6);
			this.tableLayoutPanel2.Controls.Add(this.lblSelection, 1, 7);
			this.tableLayoutPanel2.Controls.Add(this.lblAutomationId, 1, 8);
			this.tableLayoutPanel2.Controls.Add(this.lblTitle, 1, 10);
			this.tableLayoutPanel2.Controls.Add(this.lblWinClassName, 1, 11);
			this.tableLayoutPanel2.Controls.Add(this.lblProcessName, 1, 12);
			this.tableLayoutPanel2.Controls.Add(this.lblNote, 1, 13);
			this.tableLayoutPanel2.Controls.Add(this.button1, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(403, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 15;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(712, 735);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 120);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(69, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "AutomationId";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 192);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(30, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "Note";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 179);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(45, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Process";
			// 
			// pbScreenshot
			// 
			this.tableLayoutPanel2.SetColumnSpan(this.pbScreenshot, 2);
			this.pbScreenshot.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbScreenshot.Location = new System.Drawing.Point(3, 208);
			this.pbScreenshot.Name = "pbScreenshot";
			this.pbScreenshot.Size = new System.Drawing.Size(706, 524);
			this.pbScreenshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pbScreenshot.TabIndex = 5;
			this.pbScreenshot.TabStop = false;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(3, 107);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(51, 13);
			this.label14.TabIndex = 10;
			this.label14.Text = "Selection";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 166);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "ClassName";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(3, 94);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(50, 13);
			this.label13.TabIndex = 9;
			this.label13.Text = "HelpText";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 153);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(27, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Title";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(3, 81);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(64, 13);
			this.label12.TabIndex = 8;
			this.label12.Text = "ControlType";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(3, 68);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(34, 13);
			this.label11.TabIndex = 7;
			this.label11.Text = "Value";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(3, 55);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(28, 13);
			this.label10.TabIndex = 6;
			this.label10.Text = "Text";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 42);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(60, 13);
			this.label9.TabIndex = 5;
			this.label9.Text = "ClassName";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 29);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label15.Location = new System.Drawing.Point(3, 0);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(47, 13);
			this.label15.TabIndex = 11;
			this.label15.Text = "Control";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label16.Location = new System.Drawing.Point(3, 133);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(52, 13);
			this.label16.TabIndex = 12;
			this.label16.Text = "Window";
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.Location = new System.Drawing.Point(78, 29);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(10, 13);
			this.lblName.TabIndex = 13;
			this.lblName.Text = "-";
			// 
			// lblClassName
			// 
			this.lblClassName.AutoSize = true;
			this.lblClassName.Location = new System.Drawing.Point(78, 42);
			this.lblClassName.Name = "lblClassName";
			this.lblClassName.Size = new System.Drawing.Size(10, 13);
			this.lblClassName.TabIndex = 14;
			this.lblClassName.Text = "-";
			// 
			// lblText
			// 
			this.lblText.AutoSize = true;
			this.lblText.Location = new System.Drawing.Point(78, 55);
			this.lblText.Name = "lblText";
			this.lblText.Size = new System.Drawing.Size(10, 13);
			this.lblText.TabIndex = 15;
			this.lblText.Text = "-";
			// 
			// lblValue
			// 
			this.lblValue.AutoSize = true;
			this.lblValue.Location = new System.Drawing.Point(78, 68);
			this.lblValue.Name = "lblValue";
			this.lblValue.Size = new System.Drawing.Size(10, 13);
			this.lblValue.TabIndex = 16;
			this.lblValue.Text = "-";
			// 
			// lblControlType
			// 
			this.lblControlType.AutoSize = true;
			this.lblControlType.Location = new System.Drawing.Point(78, 81);
			this.lblControlType.Name = "lblControlType";
			this.lblControlType.Size = new System.Drawing.Size(10, 13);
			this.lblControlType.TabIndex = 17;
			this.lblControlType.Text = "-";
			// 
			// lblHelpText
			// 
			this.lblHelpText.AutoSize = true;
			this.lblHelpText.Location = new System.Drawing.Point(78, 94);
			this.lblHelpText.Name = "lblHelpText";
			this.lblHelpText.Size = new System.Drawing.Size(10, 13);
			this.lblHelpText.TabIndex = 18;
			this.lblHelpText.Text = "-";
			// 
			// lblSelection
			// 
			this.lblSelection.AutoSize = true;
			this.lblSelection.Location = new System.Drawing.Point(78, 107);
			this.lblSelection.Name = "lblSelection";
			this.lblSelection.Size = new System.Drawing.Size(10, 13);
			this.lblSelection.TabIndex = 19;
			this.lblSelection.Text = "-";
			// 
			// lblAutomationId
			// 
			this.lblAutomationId.AutoSize = true;
			this.lblAutomationId.Location = new System.Drawing.Point(78, 120);
			this.lblAutomationId.Name = "lblAutomationId";
			this.lblAutomationId.Size = new System.Drawing.Size(10, 13);
			this.lblAutomationId.TabIndex = 20;
			this.lblAutomationId.Text = "-";
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Location = new System.Drawing.Point(78, 153);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(10, 13);
			this.lblTitle.TabIndex = 21;
			this.lblTitle.Text = "-";
			// 
			// lblWinClassName
			// 
			this.lblWinClassName.AutoSize = true;
			this.lblWinClassName.Location = new System.Drawing.Point(78, 166);
			this.lblWinClassName.Name = "lblWinClassName";
			this.lblWinClassName.Size = new System.Drawing.Size(10, 13);
			this.lblWinClassName.TabIndex = 22;
			this.lblWinClassName.Text = "-";
			// 
			// lblProcessName
			// 
			this.lblProcessName.AutoSize = true;
			this.lblProcessName.Location = new System.Drawing.Point(78, 179);
			this.lblProcessName.Name = "lblProcessName";
			this.lblProcessName.Size = new System.Drawing.Size(10, 13);
			this.lblProcessName.TabIndex = 23;
			this.lblProcessName.Text = "-";
			// 
			// lblNote
			// 
			this.lblNote.AutoSize = true;
			this.lblNote.Location = new System.Drawing.Point(78, 192);
			this.lblNote.Name = "lblNote";
			this.lblNote.Size = new System.Drawing.Size(10, 13);
			this.lblNote.TabIndex = 24;
			this.lblNote.Text = "-";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(78, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 25;
			this.button1.Text = "GetJson";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 1;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Controls.Add(this.label7, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.label8, 0, 2);
			this.tableLayoutPanel4.Controls.Add(this.tvControl, 0, 1);
			this.tableLayoutPanel4.Controls.Add(this.tvWindow, 0, 3);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(203, 3);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 4;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(194, 735);
			this.tableLayoutPanel4.TabIndex = 4;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(3, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(40, 13);
			this.label7.TabIndex = 0;
			this.label7.Text = "Control";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(3, 367);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(46, 13);
			this.label8.TabIndex = 1;
			this.label8.Text = "Window";
			// 
			// tvControl
			// 
			this.tvControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvControl.HideSelection = false;
			this.tvControl.Location = new System.Drawing.Point(3, 16);
			this.tvControl.Name = "tvControl";
			this.tvControl.Size = new System.Drawing.Size(188, 348);
			this.tvControl.TabIndex = 2;
			this.tvControl.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.HandleControlSelectionChanged);
			// 
			// tvWindow
			// 
			this.tvWindow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvWindow.HideSelection = false;
			this.tvWindow.Location = new System.Drawing.Point(3, 383);
			this.tvWindow.Name = "tvWindow";
			this.tvWindow.Size = new System.Drawing.Size(188, 349);
			this.tvWindow.TabIndex = 3;
			this.tvWindow.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.HandleWindowSelectionChanged);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1118, 741);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "MainForm";
			this.Text = "JcMonViewer";
			this.Load += new System.EventHandler(this.HandleLoaded);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbScreenshot)).EndInit();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel4.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ListBox lbCaptures;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TreeView tvControl;
		private System.Windows.Forms.TreeView tvWindow;
		private System.Windows.Forms.PictureBox pbScreenshot;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblClassName;
		private System.Windows.Forms.Label lblText;
		private System.Windows.Forms.Label lblValue;
		private System.Windows.Forms.Label lblControlType;
		private System.Windows.Forms.Label lblHelpText;
		private System.Windows.Forms.Label lblSelection;
		private System.Windows.Forms.Label lblAutomationId;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblWinClassName;
		private System.Windows.Forms.Label lblProcessName;
		private System.Windows.Forms.Label lblNote;
		private System.Windows.Forms.Button button1;
	}
}

