namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class UserDisplay
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.pWeb = new System.Windows.Forms.Panel();
			this.pPreference = new System.Windows.Forms.Panel();
			this.pQuit = new System.Windows.Forms.Panel();
			this.pEtc = new System.Windows.Forms.Panel();
			this.label1 = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 5;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.pWeb, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.pPreference, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.pQuit, 4, 0);
			this.tableLayoutPanel1.Controls.Add(this.pEtc, 3, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(617, 22);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// pWeb
			// 
			this.pWeb.AccessibleName = "Website";
			this.pWeb.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuPopup;
			this.pWeb.BackColor = System.Drawing.Color.Transparent;
			this.pWeb.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.web;
			this.pWeb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pWeb.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pWeb.Location = new System.Drawing.Point(511, 0);
			this.pWeb.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.pWeb.Name = "pWeb";
			this.pWeb.Size = new System.Drawing.Size(22, 22);
			this.pWeb.TabIndex = 3;
			this.pWeb.TabStop = true;
			this.pWeb.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
			this.pWeb.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleWebClicked);
			// 
			// pPreference
			// 
			this.pPreference.AccessibleName = "Preferences";
			this.pPreference.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
			this.pPreference.BackColor = System.Drawing.Color.Transparent;
			this.pPreference.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.icon_settings;
			this.pPreference.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pPreference.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pPreference.Location = new System.Drawing.Point(539, 0);
			this.pPreference.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.pPreference.Name = "pPreference";
			this.pPreference.Size = new System.Drawing.Size(22, 22);
			this.pPreference.TabIndex = 2;
			this.pPreference.TabStop = true;
			this.pPreference.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
			this.pPreference.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandlePreferenceClicked);
			// 
			// pQuit
			// 
			this.pQuit.BackColor = System.Drawing.Color.Transparent;
			this.pQuit.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.icon_logout;
			this.pQuit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pQuit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pQuit.Location = new System.Drawing.Point(595, 0);
			this.pQuit.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.pQuit.Name = "pQuit";
			this.pQuit.Size = new System.Drawing.Size(22, 22);
			this.pQuit.TabIndex = 1;
			this.pQuit.TabStop = true;
			this.pQuit.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
			this.pQuit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleQuitClicked);
			// 
			// pEtc
			// 
			this.pEtc.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.further_menu;
			this.pEtc.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pEtc.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pEtc.Location = new System.Drawing.Point(567, 0);
			this.pEtc.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.pEtc.Name = "pEtc";
			this.pEtc.Size = new System.Drawing.Size(22, 22);
			this.pEtc.TabIndex = 4;
			this.pEtc.TabStop = true;
			this.pEtc.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
			this.pEtc.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleEtcClicked);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.AutoWrap = false;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Open Sans", 10F, System.Drawing.FontStyle.Bold);
			this.label1.FontSize = 10F;
			this.label1.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.label1.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(505, 22);
			this.label1.TabIndex = 0;
			this.label1.Text = "Antal Róbert ";
			this.label1.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			// 
			// UserDisplay
			// 
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.Pane;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.MinimumSize = new System.Drawing.Size(0, 22);
			this.Name = "UserDisplay";
			this.Size = new System.Drawing.Size(617, 22);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SmartLabel label1;
		private System.Windows.Forms.Panel pQuit;
		private System.Windows.Forms.Panel pPreference;
		private System.Windows.Forms.Panel pWeb;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel pEtc;
	}
}
