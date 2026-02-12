using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class PluginEditForm
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
			this.gridPlugins = new System.Windows.Forms.DataGridView();
			this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.keyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.valueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.simplePluginDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.btnParams = new MetroFramework.Controls.MetroButton();
			((System.ComponentModel.ISupportInitialize)(this.gridPlugins)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.simplePluginDataBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// gridPlugins
			// 
			this.gridPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridPlugins.AutoGenerateColumns = false;
			this.gridPlugins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridPlugins.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idDataGridViewTextBoxColumn,
            this.keyDataGridViewTextBoxColumn,
            this.valueDataGridViewTextBoxColumn});
			this.gridPlugins.DataSource = this.simplePluginDataBindingSource;
			this.gridPlugins.Location = new System.Drawing.Point(17, 63);
			this.gridPlugins.Name = "gridPlugins";
			this.gridPlugins.Size = new System.Drawing.Size(381, 167);
			this.gridPlugins.TabIndex = 0;
			// 
			// idDataGridViewTextBoxColumn
			// 
			this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
			this.idDataGridViewTextBoxColumn.HeaderText = "Id";
			this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
			// 
			// keyDataGridViewTextBoxColumn
			// 
			this.keyDataGridViewTextBoxColumn.DataPropertyName = "Key";
			this.keyDataGridViewTextBoxColumn.HeaderText = "Key";
			this.keyDataGridViewTextBoxColumn.Name = "keyDataGridViewTextBoxColumn";
			// 
			// valueDataGridViewTextBoxColumn
			// 
			this.valueDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.valueDataGridViewTextBoxColumn.DataPropertyName = "Value";
			this.valueDataGridViewTextBoxColumn.HeaderText = "Value";
			this.valueDataGridViewTextBoxColumn.Name = "valueDataGridViewTextBoxColumn";
			// 
			// simplePluginDataBindingSource
			// 
			this.simplePluginDataBindingSource.DataSource = typeof(Tct.ActivityRecorderClient.View.SimplePluginData);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(241, 236);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "OK";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(322, 236);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnParams
			// 
			this.btnParams.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnParams.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnParams.Location = new System.Drawing.Point(17, 236);
			this.btnParams.Name = "btnParams";
			this.btnParams.Size = new System.Drawing.Size(124, 23);
			this.btnParams.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnParams.TabIndex = 1;
			this.btnParams.Text = "Parameterek...";
			this.btnParams.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnParams.UseSelectable = true;
			this.btnParams.Click += new System.EventHandler(this.btnParams_Click);
			// 
			// PluginEditForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(421, 272);
			this.Controls.Add(this.btnParams);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.gridPlugins);
			this.MinimumSize = new System.Drawing.Size(421, 200);
			this.Name = "PluginEditForm";
			this.Text = "Kiegeszitok kezelese";
			((System.ComponentModel.ISupportInitialize)(this.gridPlugins)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.simplePluginDataBindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView gridPlugins;
		private System.Windows.Forms.BindingSource simplePluginDataBindingSource;
		private MetroButton btnOk;
		private MetroButton btnCancel;
		private MetroButton btnParams;
		private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn keyDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn valueDataGridViewTextBoxColumn;
	}
}