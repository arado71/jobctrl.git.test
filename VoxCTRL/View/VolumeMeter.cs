using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VoxCTRL.View
{
	/// <summary>
	/// Implements a rudimentary volume meter
	/// </summary>
	public partial class VolumeMeter : Control
	{
		LinearGradientBrush brH;
		LinearGradientBrush brV;

		/// <summary>
		/// Basic volume meter
		/// </summary>
		public VolumeMeter()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
				ControlStyles.OptimizedDoubleBuffer, true);
			MinDb = -60;
			MaxDb = 18;
			Amplitude = 0;
			Orientation = Orientation.Vertical;
			InitializeComponent();
			OnForeColorChanged(EventArgs.Empty);
			OnResize(EventArgs.Empty);
		}

		/// <summary>
		/// On Resize
		/// </summary>
		protected override void OnResize(EventArgs e)
		{
			int width = this.Width - 2;
			int height = this.Height - 2;

			if (brH != null) brH.Dispose();
			brH = new LinearGradientBrush(new Rectangle(1, 1, width, height), Color.Black, Color.Black, 0f);
			ColorBlend cbH = new ColorBlend();
			cbH.Positions = new[] { 0, 1 / 3f, 2 / 3f, 1 };
			cbH.Colors = new[] { Color.Green, Color.Yellow, Color.Orange, Color.Red, };
			brH.InterpolationColors = cbH;

			if (brV != null) brV.Dispose();
			brV = new LinearGradientBrush(new Rectangle(1, 1, width, height), Color.Black, Color.Black, 90f);
			ColorBlend cbV = new ColorBlend();
			cbV.Positions = new[] { 0, 1 / 3f, 2 / 3f, 1 };
			cbV.Colors = new[] { Color.Red, Color.Orange, Color.Yellow, Color.Green };
			brV.InterpolationColors = cbV;

			base.OnResize(e);
		}

		private float amplitude;

		/// <summary>
		/// Current Value
		/// </summary>
		[DefaultValue(-3.0)]
		public float Amplitude
		{
			get { return amplitude; }
			set
			{
				amplitude = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Minimum decibels
		/// </summary>
		[DefaultValue(-60.0)]
		public float MinDb { get; set; }

		/// <summary>
		/// Maximum decibels
		/// </summary>
		[DefaultValue(18.0)]
		public float MaxDb { get; set; }

		/// <summary>
		/// Meter orientation
		/// </summary>
		[DefaultValue(Orientation.Vertical)]
		public Orientation Orientation { get; set; }

		/// <summary>
		/// Paints the volume meter
		/// </summary>
		protected override void OnPaint(PaintEventArgs pe)
		{
			//base.OnPaint(pe);

			pe.Graphics.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);

			double db = 20 * Math.Log10(Amplitude);
			if (db < MinDb)
				db = MinDb;
			if (db > MaxDb)
				db = MaxDb;
			double percent = (db - MinDb) / (MaxDb - MinDb);

			int width = this.Width - 2;
			int height = this.Height - 2;
			if (Orientation == Orientation.Horizontal)
			{
				width = (int)(width * percent);
				pe.Graphics.FillRectangle(brH, 1, 1, width, height);

				//pe.Graphics.FillRectangle(foregroundBrush, 1, 1, width, height);
			}
			else
			{
				height = (int)(height * percent);
				pe.Graphics.FillRectangle(brV, 1, this.Height - 1 - height, width, height);

				//pe.Graphics.FillRectangle(foregroundBrush, 1, this.Height - 1 - height, width, height);
			}

			/*
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Alignment = StringAlignment.Center;
			string dbValue = String.Format("{0:F2} dB", db);
			if(Double.IsNegativeInfinity(db))
			{
				dbValue = "-\x221e db"; // -8 dB
			}

			pe.Graphics.DrawString(dbValue, this.Font,
				Brushes.Black, this.ClientRectangle, format);*/

		}
	}
}
