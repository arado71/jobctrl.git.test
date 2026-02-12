using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VoxCTRL.View
{
	/// <summary>
	/// Windows Forms control for painting audio waveforms
	/// </summary>
	public partial class WaveformPainter : Control
	{
		Pen foregroundPen;
		Pen tickPen = new Pen(Color.FromArgb(255, 36, 36, 36));
		List<float> samples = new List<float>(1000);
		int width;
		int insertPos;

		/// <summary>
		/// Constructs a new instance of the WaveFormPainter class
		/// </summary>
		public WaveformPainter()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
			InitializeComponent();
			OnForeColorChanged(EventArgs.Empty);
			OnResize(EventArgs.Empty);
		}

		/// <summary>
		/// On Resize
		/// </summary>
		protected override void OnResize(EventArgs e)
		{
			width = this.Width;
			base.OnResize(e);
		}

		/// <summary>
		/// On ForeColor Changed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnForeColorChanged(EventArgs e)
		{
			foregroundPen = new Pen(ForeColor);
			base.OnForeColorChanged(e);
		}

		/// <summary>
		/// Add Max Value
		/// </summary>
		/// <param name="maxSample"></param>
		public void AddMax(float maxSample)
		{
			if (width == 0)
			{
				// sometimes when you minimise, max samples can be set to 0
				return;
			}
			if (samples.Count <= width)
			{
				samples.Add(maxSample);
			}
			else if (insertPos < width)
			{
				samples[insertPos] = maxSample;
			}
			insertPos++;
			insertPos %= width;

			this.Invalidate();
		}

		public void Clear()
		{
			samples.Clear();
			insertPos = 0;
			this.Invalidate();
		}

		/// <summary>
		/// On Paint
		/// </summary>
		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);

			for (int x = 0; x < this.Width; x += 10)
			{
				pe.Graphics.DrawLine(tickPen, x, 5, x, this.Height - 8);
			}

			for (int x = 0; x < this.Width; x++)
			{
				float lineHeight = this.Height * GetSample(x - this.Width + insertPos);
				float y1 = (this.Height - lineHeight) / 2;
				pe.Graphics.DrawLine(foregroundPen, x, y1, x, y1 + lineHeight);
			}
		}

		float GetSample(int index)
		{
			if (index < 0)
				index += width;
			if (index >= 0 & index < samples.Count)
				return samples[index];
			return 0;
		}
	}
}
