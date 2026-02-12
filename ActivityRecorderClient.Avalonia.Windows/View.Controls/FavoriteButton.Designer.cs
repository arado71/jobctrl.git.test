namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class FavoriteButton
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
			this.SuspendLayout();
			// 
			// FavoriteButton
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.favorite;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DoubleBuffered = true;
			this.Margin = new System.Windows.Forms.Padding(0);
			this.MaximumSize = new System.Drawing.Size(16, 16);
			this.MinimumSize = new System.Drawing.Size(16, 16);
			this.Name = "FavoriteButton";
			this.Size = new System.Drawing.Size(16, 16);
			this.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
