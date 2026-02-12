using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed partial class ScrollBar : UserControl
	{
		private static readonly SolidBrush altBrush = new SolidBrush(StyleUtils.Foreground);
		private static readonly SolidBrush brush = new SolidBrush(StyleUtils.ForegroundLight);
		private int dragStartPos = 0;
		private int dragStartVal = 0;
		private bool dragging = false;
		private bool mouseOver = false;
		private int scrollTotalSize = 100;
		private int scrollVisibleSize = 10;
		private int value = 0;

		public event EventHandler CloseToEnd;
		public event EventHandler ScrollChanged;

		public float ScrollSpeed { get; set; }

		public int ScrollTotalSize
		{
			get { return scrollTotalSize; }

			set
			{
				scrollTotalSize = value;
				Visible = scrollTotalSize > scrollVisibleSize;
				Invalidate();
			}
		}

		public int ScrollVisibleSize
		{
			get { return scrollVisibleSize; }

			set
			{
				scrollVisibleSize = value;
				Visible = scrollTotalSize > scrollVisibleSize;
				Invalidate();
			}
		}

		public void SetScrollSize(int visibleSize, int totalSize)
		{
			if (visibleSize == scrollVisibleSize && totalSize == scrollTotalSize) return;
			scrollVisibleSize = visibleSize;
			scrollTotalSize = totalSize;
			if (Visible == (scrollTotalSize > scrollVisibleSize)) return;
			Visible = scrollTotalSize > scrollVisibleSize;
			Invalidate();
		}

		public int Value
		{
			get { return value; }
			set
			{
				this.value = Math.Max(Math.Min(value, scrollTotalSize - scrollVisibleSize), 0);
				if (this.value > scrollTotalSize - scrollVisibleSize*1.5)
				{
					EventHandler evt = CloseToEnd;
					if (evt != null) evt(this, EventArgs.Empty);
				}

				Invalidate();
			}
		}

		public ScrollBar()
		{
			InitializeComponent();
			DoubleBuffered = true;
			ScrollSpeed = 10;
		}

		public void ScrollDelta(int delta)
		{
			Value -= (int) (delta*ScrollSpeed);
			RaiseScrollChanged();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			dragStartPos = e.Y;
			dragStartVal = Value;
			dragging = true;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (dragging)
			{
				Value = dragStartVal + (int) ((e.Y - dragStartPos)/GetCoeff());
				RaiseScrollChanged();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			dragging = false;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			ScrollDelta(e.Delta);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			SolidBrush currentBrush = mouseOver ? altBrush : brush;
			Graphics g = e.Graphics;
			using (var bgBrush = new SolidBrush(StyleUtils.Background))
			{
				g.FillRectangle(bgBrush, new Rectangle(Point.Empty, Size));
			}

			g.CompositingQuality = CompositingQuality.HighQuality;
			int scrollableSize = Math.Max(ScrollTotalSize - ScrollVisibleSize, 0);
			double coefficient = GetCoeff();
			int currentHeight = scrollableSize != 0 ? (int) (Value*coefficient) : 0;
			var scrollHeight = (int) (ScrollVisibleSize*coefficient);
			g.FillEllipse(currentBrush, 0, currentHeight, Width-1, Width-1);
			if (scrollHeight > Width)
			{
				g.FillRectangle(currentBrush, 0, currentHeight + Width/2, Width, scrollHeight - Width + 1);
				g.FillEllipse(currentBrush, 0, currentHeight + scrollHeight - Width, Width-1, Width-1);
			}
		}

		private void HandleMouseEntered(object sender, EventArgs e)
		{
			mouseOver = true;
			Invalidate();
		}

		private void HandleMouseLeft(object sender, EventArgs e)
		{
			mouseOver = false;
			Invalidate();
		}

		private double GetCoeff()
		{
			int scrollableSize = ScrollTotalSize - ScrollVisibleSize;
			return (Height/(double) (scrollableSize + ScrollVisibleSize));
		}

		private void RaiseScrollChanged()
		{
			EventHandler evt = ScrollChanged;
			if (evt != null) evt(this, EventArgs.Empty);
		}
	}
}