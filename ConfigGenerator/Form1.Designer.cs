namespace ConfigGenerator
{
	partial class Form1
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
			this.lbCompanyName = new System.Windows.Forms.Label();
			this.lbConfigName = new System.Windows.Forms.Label();
			this.tbCompanyName = new System.Windows.Forms.TextBox();
			this.tbConfigName = new System.Windows.Forms.TextBox();
			this.lbDisplayName = new System.Windows.Forms.Label();
			this.tbDisplayName = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.chOcr = new System.Windows.Forms.CheckBox();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.cbConfigKey1 = new System.Windows.Forms.ComboBox();
			this.tbConfigValue1 = new System.Windows.Forms.TextBox();
			this.btnAddRow = new System.Windows.Forms.Button();
			this.btnGenerate = new System.Windows.Forms.Button();
			this.btnLoad = new System.Windows.Forms.Button();
			this.lbLoadedConfig = new System.Windows.Forms.Label();
			this.clbEndpoints = new System.Windows.Forms.CheckedListBox();
			this.lbHost = new System.Windows.Forms.Label();
			this.tbHost = new System.Windows.Forms.TextBox();
			this.lbUrl = new System.Windows.Forms.Label();
			this.lbPublicKey = new System.Windows.Forms.Label();
			this.tbUrl = new System.Windows.Forms.TextBox();
			this.tbPublicKey = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.73913F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.26087F));
			this.tableLayoutPanel1.Controls.Add(this.lbCompanyName, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.lbConfigName, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tbCompanyName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.tbConfigName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.lbDisplayName, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.tbDisplayName, 1, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 73);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(500, 81);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lbCompanyName
			// 
			this.lbCompanyName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbCompanyName.AutoSize = true;
			this.lbCompanyName.Location = new System.Drawing.Point(3, 0);
			this.lbCompanyName.Name = "lbCompanyName";
			this.lbCompanyName.Size = new System.Drawing.Size(102, 27);
			this.lbCompanyName.TabIndex = 0;
			this.lbCompanyName.Text = "Company name";
			this.lbCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lbConfigName
			// 
			this.lbConfigName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbConfigName.AutoSize = true;
			this.lbConfigName.Location = new System.Drawing.Point(3, 27);
			this.lbConfigName.Name = "lbConfigName";
			this.lbConfigName.Size = new System.Drawing.Size(102, 27);
			this.lbConfigName.TabIndex = 1;
			this.lbConfigName.Text = "Config name";
			this.lbConfigName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tbCompanyName
			// 
			this.tbCompanyName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbCompanyName.Location = new System.Drawing.Point(111, 3);
			this.tbCompanyName.Name = "tbCompanyName";
			this.tbCompanyName.Size = new System.Drawing.Size(386, 20);
			this.tbCompanyName.TabIndex = 2;
			this.tbCompanyName.TextChanged += new System.EventHandler(this.tbCompanyName_TextChanged);
			// 
			// tbConfigName
			// 
			this.tbConfigName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbConfigName.Location = new System.Drawing.Point(111, 30);
			this.tbConfigName.Name = "tbConfigName";
			this.tbConfigName.Size = new System.Drawing.Size(386, 20);
			this.tbConfigName.TabIndex = 3;
			// 
			// lbDisplayName
			// 
			this.lbDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbDisplayName.AutoSize = true;
			this.lbDisplayName.Location = new System.Drawing.Point(3, 54);
			this.lbDisplayName.Name = "lbDisplayName";
			this.lbDisplayName.Size = new System.Drawing.Size(102, 27);
			this.lbDisplayName.TabIndex = 4;
			this.lbDisplayName.Text = "Display name";
			this.lbDisplayName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tbDisplayName
			// 
			this.tbDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbDisplayName.Location = new System.Drawing.Point(111, 57);
			this.tbDisplayName.Name = "tbDisplayName";
			this.tbDisplayName.Size = new System.Drawing.Size(386, 20);
			this.tbDisplayName.TabIndex = 5;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Controls.Add(this.chOcr, 0, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(13, 329);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(107, 48);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// chOcr
			// 
			this.chOcr.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chOcr.AutoSize = true;
			this.chOcr.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.chOcr.Location = new System.Drawing.Point(3, 3);
			this.chOcr.Name = "chOcr";
			this.chOcr.Size = new System.Drawing.Size(101, 18);
			this.chOcr.TabIndex = 0;
			this.chOcr.Text = "OCR";
			this.chOcr.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Controls.Add(this.cbConfigKey1, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.tbConfigValue1, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.btnAddRow, 0, 1);
			this.tableLayoutPanel3.Location = new System.Drawing.Point(13, 383);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.Size = new System.Drawing.Size(500, 55);
			this.tableLayoutPanel3.TabIndex = 2;
			// 
			// cbConfigKey1
			// 
			this.cbConfigKey1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cbConfigKey1.FormattingEnabled = true;
			this.cbConfigKey1.Location = new System.Drawing.Point(3, 3);
			this.cbConfigKey1.Name = "cbConfigKey1";
			this.cbConfigKey1.Size = new System.Drawing.Size(244, 21);
			this.cbConfigKey1.Sorted = true;
			this.cbConfigKey1.TabIndex = 0;
			// 
			// tbConfigValue1
			// 
			this.tbConfigValue1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbConfigValue1.Location = new System.Drawing.Point(253, 3);
			this.tbConfigValue1.Name = "tbConfigValue1";
			this.tbConfigValue1.Size = new System.Drawing.Size(244, 20);
			this.tbConfigValue1.TabIndex = 1;
			// 
			// btnAddRow
			// 
			this.btnAddRow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddRow.Location = new System.Drawing.Point(3, 30);
			this.btnAddRow.Name = "btnAddRow";
			this.btnAddRow.Size = new System.Drawing.Size(244, 22);
			this.btnAddRow.TabIndex = 2;
			this.btnAddRow.Text = "Add row";
			this.btnAddRow.UseVisualStyleBackColor = true;
			this.btnAddRow.Click += new System.EventHandler(this.btnAddRow_Click);
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(13, 42);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(75, 23);
			this.btnGenerate.TabIndex = 3;
			this.btnGenerate.Text = "Generate";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(13, 13);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(75, 23);
			this.btnLoad.TabIndex = 4;
			this.btnLoad.Text = "Load";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
			// 
			// lbLoadedConfig
			// 
			this.lbLoadedConfig.AutoSize = true;
			this.lbLoadedConfig.Location = new System.Drawing.Point(107, 18);
			this.lbLoadedConfig.Name = "lbLoadedConfig";
			this.lbLoadedConfig.Size = new System.Drawing.Size(0, 13);
			this.lbLoadedConfig.TabIndex = 5;
			// 
			// clbEndpoints
			// 
			this.clbEndpoints.CheckOnClick = true;
			this.clbEndpoints.ColumnWidth = 250;
			this.clbEndpoints.FormattingEnabled = true;
			this.clbEndpoints.Location = new System.Drawing.Point(13, 187);
			this.clbEndpoints.MultiColumn = true;
			this.clbEndpoints.Name = "clbEndpoints";
			this.clbEndpoints.Size = new System.Drawing.Size(500, 79);
			this.clbEndpoints.TabIndex = 6;
			this.clbEndpoints.SelectedIndexChanged += new System.EventHandler(this.clbEndpoints_SelectedIndexChanged);
			// 
			// lbHost
			// 
			this.lbHost.AutoSize = true;
			this.lbHost.Location = new System.Drawing.Point(13, 168);
			this.lbHost.Name = "lbHost";
			this.lbHost.Size = new System.Drawing.Size(29, 13);
			this.lbHost.TabIndex = 7;
			this.lbHost.Text = "Host";
			// 
			// tbHost
			// 
			this.tbHost.Location = new System.Drawing.Point(124, 165);
			this.tbHost.Name = "tbHost";
			this.tbHost.Size = new System.Drawing.Size(389, 20);
			this.tbHost.TabIndex = 8;
			// 
			// lbUrl
			// 
			this.lbUrl.AutoSize = true;
			this.lbUrl.Location = new System.Drawing.Point(16, 279);
			this.lbUrl.Name = "lbUrl";
			this.lbUrl.Size = new System.Drawing.Size(20, 13);
			this.lbUrl.TabIndex = 9;
			this.lbUrl.Text = "Url";
			// 
			// lbPublicKey
			// 
			this.lbPublicKey.AutoSize = true;
			this.lbPublicKey.Location = new System.Drawing.Point(16, 306);
			this.lbPublicKey.Name = "lbPublicKey";
			this.lbPublicKey.Size = new System.Drawing.Size(56, 13);
			this.lbPublicKey.TabIndex = 10;
			this.lbPublicKey.Text = "Public key";
			// 
			// tbUrl
			// 
			this.tbUrl.Enabled = false;
			this.tbUrl.Location = new System.Drawing.Point(110, 276);
			this.tbUrl.Name = "tbUrl";
			this.tbUrl.Size = new System.Drawing.Size(403, 20);
			this.tbUrl.TabIndex = 11;
			// 
			// tbPublicKey
			// 
			this.tbPublicKey.Enabled = false;
			this.tbPublicKey.Location = new System.Drawing.Point(110, 303);
			this.tbPublicKey.Name = "tbPublicKey";
			this.tbPublicKey.Size = new System.Drawing.Size(403, 20);
			this.tbPublicKey.TabIndex = 12;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(555, 550);
			this.Controls.Add(this.tbPublicKey);
			this.Controls.Add(this.tbUrl);
			this.Controls.Add(this.lbPublicKey);
			this.Controls.Add(this.lbUrl);
			this.Controls.Add(this.tbHost);
			this.Controls.Add(this.lbHost);
			this.Controls.Add(this.clbEndpoints);
			this.Controls.Add(this.lbLoadedConfig);
			this.Controls.Add(this.btnLoad);
			this.Controls.Add(this.btnGenerate);
			this.Controls.Add(this.tableLayoutPanel3);
			this.Controls.Add(this.tableLayoutPanel2);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "Form1";
			this.Text = "JobCTRL config generator";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lbCompanyName;
		private System.Windows.Forms.Label lbConfigName;
		private System.Windows.Forms.TextBox tbCompanyName;
		private System.Windows.Forms.TextBox tbConfigName;
		private System.Windows.Forms.Label lbDisplayName;
		private System.Windows.Forms.TextBox tbDisplayName;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.CheckBox chOcr;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.ComboBox cbConfigKey1;
		private System.Windows.Forms.TextBox tbConfigValue1;
		private System.Windows.Forms.Button btnAddRow;
		private System.Windows.Forms.Button btnGenerate;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.Label lbLoadedConfig;
		private System.Windows.Forms.CheckedListBox clbEndpoints;
		private System.Windows.Forms.Label lbHost;
		private System.Windows.Forms.TextBox tbHost;
		private System.Windows.Forms.Label lbUrl;
		private System.Windows.Forms.Label lbPublicKey;
		private System.Windows.Forms.TextBox tbUrl;
		private System.Windows.Forms.TextBox tbPublicKey;
	}
}

