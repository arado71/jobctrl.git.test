namespace Tct.ActivityRecorderClient.View
{
	partial class MessageView
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.messageListDataGridView = new System.Windows.Forms.DataGridView();
			this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colReasonText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.messageViewerRichTextBox = new System.Windows.Forms.RichTextBox();
			this.closeButton = new MetroFramework.Controls.MetroButton();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.messageListDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// messageListDataGridView
			// 
			this.messageListDataGridView.AllowUserToAddRows = false;
			this.messageListDataGridView.AllowUserToDeleteRows = false;
			this.messageListDataGridView.AllowUserToResizeColumns = false;
			this.messageListDataGridView.AllowUserToResizeRows = false;
			this.messageListDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.messageListDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
			this.messageListDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageListDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.messageListDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.messageListDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.messageListDataGridView.ColumnHeadersVisible = false;
			this.messageListDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colDate,
            this.colReasonText});
			this.messageListDataGridView.GridColor = System.Drawing.SystemColors.Window;
			this.messageListDataGridView.Location = new System.Drawing.Point(10, 64);
			this.messageListDataGridView.MultiSelect = false;
			this.messageListDataGridView.Name = "messageListDataGridView";
			this.messageListDataGridView.ReadOnly = true;
			this.messageListDataGridView.RowHeadersVisible = false;
			this.messageListDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.messageListDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.messageListDataGridView.ShowEditingIcon = false;
			this.messageListDataGridView.ShowRowErrors = false;
			this.messageListDataGridView.Size = new System.Drawing.Size(269, 295);
			this.messageListDataGridView.TabIndex = 0;
			this.messageListDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.messageListDataGridView_CellDoubleClick);
			this.messageListDataGridView.SelectionChanged += new System.EventHandler(this.messageList_SelectionChanged);
			// 
			// colId
			// 
			this.colId.HeaderText = "id";
			this.colId.Name = "colId";
			this.colId.ReadOnly = true;
			this.colId.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.colId.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colId.Visible = false;
			// 
			// colDate
			// 
			this.colDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
			this.colDate.DefaultCellStyle = dataGridViewCellStyle1;
			this.colDate.HeaderText = "idopont";
			this.colDate.Name = "colDate";
			this.colDate.ReadOnly = true;
			this.colDate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.colDate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colDate.Width = 5;
			// 
			// colReasonText
			// 
			this.colReasonText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
			this.colReasonText.DefaultCellStyle = dataGridViewCellStyle2;
			this.colReasonText.FillWeight = 50F;
			this.colReasonText.HeaderText = "szoveg";
			this.colReasonText.MinimumWidth = 50;
			this.colReasonText.Name = "colReasonText";
			this.colReasonText.ReadOnly = true;
			this.colReasonText.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.colReasonText.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// messageViewerRichTextBox
			// 
			this.messageViewerRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageViewerRichTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.messageViewerRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageViewerRichTextBox.Location = new System.Drawing.Point(285, 63);
			this.messageViewerRichTextBox.Name = "messageViewerRichTextBox";
			this.messageViewerRichTextBox.ReadOnly = true;
			this.messageViewerRichTextBox.Size = new System.Drawing.Size(448, 296);
			this.messageViewerRichTextBox.TabIndex = 1;
			this.messageViewerRichTextBox.Text = "";
			this.messageViewerRichTextBox.WordWrap = false;
			this.messageViewerRichTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.messageViewerRichTextBox_LinkClicked);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(658, 365);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 2;
			this.closeButton.Text = "bezárás";
			this.closeButton.UseSelectable = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.HeaderText = "id";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.dataGridViewTextBoxColumn1.Visible = false;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn2.HeaderText = "idopont";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn3.FillWeight = 50F;
			this.dataGridViewTextBoxColumn3.HeaderText = "szoveg";
			this.dataGridViewTextBoxColumn3.MinimumWidth = 50;
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// MessageView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(741, 405);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.messageViewerRichTextBox);
			this.Controls.Add(this.messageListDataGridView);
			this.MinimumSize = new System.Drawing.Size(400, 100);
			this.Name = "MessageView";
			this.Text = "MessageView";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MessageView_FormClosing);
			this.Load += new System.EventHandler(this.MessageView_Load);
			((System.ComponentModel.ISupportInitialize)(this.messageListDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView messageListDataGridView;
		private System.Windows.Forms.RichTextBox messageViewerRichTextBox;
		private MetroFramework.Controls.MetroButton closeButton;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn colId;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReasonText;
	}
}