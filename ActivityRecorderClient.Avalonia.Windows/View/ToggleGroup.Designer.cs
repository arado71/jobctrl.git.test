namespace Tct.ActivityRecorderClient.View
{
	partial class ToggleGroup
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
			this.lblTitle = new MetroFramework.Controls.MetroLabel();
			this.cbToggle = new MetroFramework.Controls.MetroToggle();
			this.lblState = new MetroFramework.Controls.MetroLabel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.FontSize = MetroFramework.MetroLabelSize.Medium;
			this.lblTitle.FontWeight = MetroFramework.MetroLabelWeight.Light;
			this.lblTitle.LabelMode = MetroFramework.Controls.MetroLabelMode.Default;
			this.lblTitle.Location = new System.Drawing.Point(0, 1);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(190, 20);
			this.lblTitle.Style = MetroFramework.MetroColorStyle.Orange;
			this.lblTitle.StyleManager = null;
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Title";
			this.lblTitle.Theme = MetroFramework.MetroThemeStyle.Light;
			this.lblTitle.UseStyleColors = false;
			// 
			// cbToggle
			// 
			this.cbToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbToggle.AutoSize = true;
			this.cbToggle.DisplayStatus = false;
			this.cbToggle.FontSize = MetroFramework.MetroLinkSize.Small;
			this.cbToggle.FontWeight = MetroFramework.MetroLinkWeight.Regular;
			this.cbToggle.Location = new System.Drawing.Point(350, 3);
			this.cbToggle.Name = "cbToggle";
			this.cbToggle.Size = new System.Drawing.Size(50, 17);
			this.cbToggle.Style = MetroFramework.MetroColorStyle.Blue;
			this.cbToggle.StyleManager = null;
			this.cbToggle.TabIndex = 1;
			this.cbToggle.Text = "Off";
			this.cbToggle.Theme = MetroFramework.MetroThemeStyle.Light;
			this.cbToggle.UseStyleColors = false;
			this.cbToggle.UseVisualStyleBackColor = true;
			// 
			// lblState
			// 
			this.lblState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblState.FontSize = MetroFramework.MetroLabelSize.Medium;
			this.lblState.FontWeight = MetroFramework.MetroLabelWeight.Light;
			this.lblState.LabelMode = MetroFramework.Controls.MetroLabelMode.Default;
			this.lblState.Location = new System.Drawing.Point(199, 0);
			this.lblState.Name = "lblState";
			this.lblState.Size = new System.Drawing.Size(145, 22);
			this.lblState.Style = MetroFramework.MetroColorStyle.Orange;
			this.lblState.StyleManager = null;
			this.lblState.TabIndex = 2;
			this.lblState.Text = "Off";
			this.lblState.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.lblState.Theme = MetroFramework.MetroThemeStyle.Light;
			this.lblState.UseStyleColors = false;
			// 
			// ToggleGroup
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.lblState);
			this.Controls.Add(this.cbToggle);
			this.Controls.Add(this.lblTitle);
			this.MinimumSize = new System.Drawing.Size(400, 22);
			this.Name = "ToggleGroup";
			this.Size = new System.Drawing.Size(400, 22);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MetroFramework.Controls.MetroLabel lblTitle;
		private MetroFramework.Controls.MetroToggle cbToggle;
		private MetroFramework.Controls.MetroLabel lblState;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}
