namespace Tct.ActivityRecorderClient.View
{
	partial class ModifyIntervalForm
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
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblStart = new MetroFramework.Controls.MetroLabel();
			this.lblEnd = new MetroFramework.Controls.MetroLabel();
			this.dtpLocalStart = new System.Windows.Forms.DateTimePicker();
			this.dtpLocalEnd = new System.Windows.Forms.DateTimePicker();
			this.lblWork = new MetroFramework.Controls.MetroLabel();
			this.workSelector = new Tct.ActivityRecorderClient.View.WorkSelectorComboBox();
			this.lblComment = new MetroFramework.Controls.MetroLabel();
			this.txtComment = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.rbAdd = new System.Windows.Forms.RadioButton();
			this.rbModify = new System.Windows.Forms.RadioButton();
			this.rbDelete = new System.Windows.Forms.RadioButton();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.lblStart, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblEnd, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.dtpLocalStart, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.dtpLocalEnd, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblWork, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.workSelector, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblComment, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.txtComment, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 60);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(15, 10, 10, 10);
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(513, 290);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lblStart
			// 
			this.lblStart.AutoSize = true;
			this.lblStart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStart.Location = new System.Drawing.Point(18, 37);
			this.lblStart.Name = "lblStart";
			this.lblStart.Size = new System.Drawing.Size(238, 19);
			this.lblStart.TabIndex = 0;
			this.lblStart.Text = "Start";
			// 
			// lblEnd
			// 
			this.lblEnd.AutoSize = true;
			this.lblEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEnd.Location = new System.Drawing.Point(269, 37);
			this.lblEnd.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
			this.lblEnd.Name = "lblEnd";
			this.lblEnd.Size = new System.Drawing.Size(231, 19);
			this.lblEnd.TabIndex = 1;
			this.lblEnd.Text = "End";
			// 
			// dtpLocalStart
			// 
			this.dtpLocalStart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dtpLocalStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			this.errorProvider1.SetIconAlignment(this.dtpLocalStart, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
			this.dtpLocalStart.Location = new System.Drawing.Point(18, 59);
			this.dtpLocalStart.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.dtpLocalStart.Name = "dtpLocalStart";
			this.dtpLocalStart.Size = new System.Drawing.Size(231, 20);
			this.dtpLocalStart.TabIndex = 3;
			this.dtpLocalStart.ValueChanged += new System.EventHandler(this.HandleDateChanged);
			// 
			// dtpLocalEnd
			// 
			this.dtpLocalEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dtpLocalEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			this.errorProvider1.SetIconAlignment(this.dtpLocalEnd, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
			this.dtpLocalEnd.Location = new System.Drawing.Point(269, 59);
			this.dtpLocalEnd.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.dtpLocalEnd.Name = "dtpLocalEnd";
			this.dtpLocalEnd.Size = new System.Drawing.Size(231, 20);
			this.dtpLocalEnd.TabIndex = 4;
			this.dtpLocalEnd.ValueChanged += new System.EventHandler(this.HandleDateChanged);
			// 
			// lblWork
			// 
			this.lblWork.AutoSize = true;
			this.lblWork.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblWork.Location = new System.Drawing.Point(18, 82);
			this.lblWork.Name = "lblWork";
			this.lblWork.Size = new System.Drawing.Size(238, 19);
			this.lblWork.TabIndex = 4;
			this.lblWork.Text = "Work";
			// 
			// workSelector
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.workSelector, 2);
			this.workSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workSelector.DropDownWidth = 482;
			this.workSelector.FormattingEnabled = true;
			this.errorProvider1.SetIconAlignment(this.workSelector, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
			this.workSelector.IntegralHeight = false;
			this.workSelector.Location = new System.Drawing.Point(18, 104);
			this.workSelector.MaxDropDownItems = 30;
			this.workSelector.Name = "workSelector";
			this.workSelector.Size = new System.Drawing.Size(482, 21);
			this.workSelector.TabIndex = 5;
			// 
			// lblComment
			// 
			this.lblComment.AutoSize = true;
			this.lblComment.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblComment.Location = new System.Drawing.Point(18, 128);
			this.lblComment.Name = "lblComment";
			this.lblComment.Size = new System.Drawing.Size(238, 19);
			this.lblComment.TabIndex = 6;
			this.lblComment.Text = "Comment";
			// 
			// txtComment
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.txtComment, 2);
			this.txtComment.Dock = System.Windows.Forms.DockStyle.Fill;
			this.errorProvider1.SetIconAlignment(this.txtComment, System.Windows.Forms.ErrorIconAlignment.TopLeft);
			this.txtComment.Location = new System.Drawing.Point(18, 150);
			this.txtComment.Multiline = true;
			this.txtComment.Name = "txtComment";
			this.txtComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtComment.Size = new System.Drawing.Size(482, 91);
			this.txtComment.TabIndex = 6;
			// 
			// flowLayoutPanel2
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel2, 2);
			this.flowLayoutPanel2.Controls.Add(this.btnCancel);
			this.flowLayoutPanel2.Controls.Add(this.btnOk);
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(18, 247);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(482, 30);
			this.flowLayoutPanel2.TabIndex = 10;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(404, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 8;
			this.btnCancel.Text = "Mégse";
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(323, 3);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 7;
			this.btnOk.Text = "OK";
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// flowLayoutPanel1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
			this.flowLayoutPanel1.Controls.Add(this.rbAdd);
			this.flowLayoutPanel1.Controls.Add(this.rbModify);
			this.flowLayoutPanel1.Controls.Add(this.rbDelete);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(18, 13);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(482, 21);
			this.flowLayoutPanel1.TabIndex = 11;
			// 
			// rbAdd
			// 
			this.rbAdd.AutoSize = true;
			this.rbAdd.Checked = true;
			this.rbAdd.Location = new System.Drawing.Point(3, 3);
			this.rbAdd.Name = "rbAdd";
			this.rbAdd.Size = new System.Drawing.Size(44, 17);
			this.rbAdd.TabIndex = 0;
			this.rbAdd.TabStop = true;
			this.rbAdd.Text = "Add";
			this.rbAdd.UseVisualStyleBackColor = true;
			this.rbAdd.CheckedChanged += new System.EventHandler(this.HandleRadioChanged);
			// 
			// rbModify
			// 
			this.rbModify.AutoSize = true;
			this.rbModify.Location = new System.Drawing.Point(53, 3);
			this.rbModify.Name = "rbModify";
			this.rbModify.Size = new System.Drawing.Size(56, 17);
			this.rbModify.TabIndex = 1;
			this.rbModify.Text = "Modify";
			this.rbModify.UseVisualStyleBackColor = true;
			this.rbModify.CheckedChanged += new System.EventHandler(this.HandleRadioChanged);
			// 
			// rbDelete
			// 
			this.rbDelete.AutoSize = true;
			this.rbDelete.Location = new System.Drawing.Point(115, 3);
			this.rbDelete.Name = "rbDelete";
			this.rbDelete.Size = new System.Drawing.Size(56, 17);
			this.rbDelete.TabIndex = 2;
			this.rbDelete.Text = "Delete";
			this.rbDelete.UseVisualStyleBackColor = true;
			this.rbDelete.CheckedChanged += new System.EventHandler(this.HandleRadioChanged);
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			// 
			// ModifyIntervalForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(553, 370);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "ModifyIntervalForm";
			this.Text = "Create work";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private MetroFramework.Controls.MetroLabel lblStart;
		private MetroFramework.Controls.MetroLabel lblEnd;
		private System.Windows.Forms.DateTimePicker dtpLocalStart;
		private System.Windows.Forms.DateTimePicker dtpLocalEnd;
		private MetroFramework.Controls.MetroLabel lblWork;
		private WorkSelectorComboBox workSelector;
		private MetroFramework.Controls.MetroLabel lblComment;
		private System.Windows.Forms.TextBox txtComment;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private MetroFramework.Controls.MetroButton btnCancel;
		private MetroFramework.Controls.MetroButton btnOk;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton rbAdd;
		private System.Windows.Forms.RadioButton rbModify;
		private System.Windows.Forms.RadioButton rbDelete;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}