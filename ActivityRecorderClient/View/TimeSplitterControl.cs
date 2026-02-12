using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Properties;
using Label = System.Windows.Forms.Label;
using Panel = System.Windows.Forms.Panel;

namespace Tct.ActivityRecorderClient.View
{
	public partial class TimeSplitterControl : UserControl
	{
		private const int splitterOffset = 15;
		private const int fixedGap = 30;
		private const int splitterMargin = 15;
		private static readonly Color caretColor = Color.FromArgb(179, 179, 179);
		private static readonly Color hoverColor = Color.FromArgb(18, 156, 221);
		private static readonly int imgHeight = Resources.locked_end.Height;
		private static int nextIndex;
		private readonly List<SplitterData> splitters = new List<SplitterData>();
		private readonly ToolTip tip = new ToolTip();
		private readonly Size labelSize;
		private SplitterData dragged;
		private SplitterData draggedPrev;
		private SplitterData draggedNext;
		private Point dragPos;
		private int dragMin, dragMax;
		private double msPerPx;
		private DateTime lastAlignedPreciseDate;

		public event EventHandler<SingleValueEventArgs<int>> IntervalSelected;
		public event EventHandler<SingleValueEventArgs<Tuple<int, DateTime>>> SplitterTimeChanged;

		public int SplitterCount => splitters.Count;

		public TimeSplitterControl()
		{
			InitializeComponent();
			pnlMain.splitters = splitters;
			this.SetStyle(ControlStyles.UserPaint |
			              ControlStyles.AllPaintingInWmPaint |
			              ControlStyles.OptimizedDoubleBuffer,
				true);
			if (components == null) components = new Container();
			components.Add(tip);
			pnlMain.SizeChanged += PnlMainSizeChanged;
			pnlMain.MouseDown += PnlMainMouseDown;
			pnlMain.MouseUp += PnlMainMouseUp;
			pnlMain.MouseMove += PnlMainMouseMove;
			pnlMain.MouseLeave += PnlMainMouseLeave;

			var sz = new Size(Int32.MaxValue, Int32.MaxValue);
			labelSize = TextRenderer.MeasureText("00:00", this.Font, sz, TextFormatFlags.SingleLine); //dpi aware

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) return;;
			AddSplitter(DateTime.Parse("2018-01-01 13:00"), Color.DarkCyan, EndType.LockedEnd);
			AddSplitter(DateTime.Parse("2018-01-01 13:13"), Color.DarkBlue, EndType.DragEnd);
			AddSplitter(DateTime.Parse("2018-01-01 13:50"), Color.BlueViolet, EndType.DragEnd);
			AddSplitter(DateTime.Parse("2018-01-01 14:00"), pnlMain.BackColor, EndType.LockedEnd, true);
			AddSplitter(DateTime.Parse("2018-01-01 16:00"), Color.DarkBlue, EndType.LockedEnd);
			AddSplitter(DateTime.Parse("2018-01-01 16:40"), Color.BlueViolet, EndType.DragEnd);
			AddSplitter(DateTime.Parse("2018-01-01 16:48"), Color.Red, EndType.LockedEnd);
			Rearrange();
		}

		private void PnlMainSizeChanged(object sender, EventArgs e)
		{
			Rearrange();
		}

		public int AddSplitter(DateTime meetingStart, Color color, bool isDraggable, bool isFixed)
		{
			var idx = AddSplitter(meetingStart, color, isDraggable ? EndType.DragEnd : EndType.LockedEnd, isFixed);
			Rearrange();
			return idx;
		}
		
		public void SetSplitterBarColor(int index, Color color)
		{
			var data = splitters.FirstOrDefault(s => s.Index == index);
			if (data == null) return;
			data.BarColor = color;
			InvalidateSplitterFraction(data);
		}

		public void SetSplitterEnd(int index, bool isDraggable, bool isFixed)
		{
			var data = splitters.FirstOrDefault(s => s.Index == index);
			if (data == null) return;
			data.SplitterEndType = isDraggable ? EndType.DragEnd : EndType.LockedEnd;
			data.IsFixed = isFixed;
			InvalidateSplitterFraction(data);
		}

		private void InvalidateSplitterFraction(SplitterData data)
		{
			var pos = splitters.IndexOf(data);
			if (pos < 0 || pos > splitters.Count - 2)
			{
				pnlMain.Invalidate();
				Debug.Fail("Splitter index invalid!");
				return;
			}
			var next = splitters[pos + 1];
			pnlMain.Invalidate(new Rectangle(data.StartX, 5, next.StartX - data.StartX, pnlMain.ClientSize.Height - 5));
		}

		public void SetSplitterStartTime(int index, DateTime start)
		{
			var data = splitters.FirstOrDefault(s => s.Index == index);
			if (data == null) return;
			data.PreciseDate = start;
			Rearrange();
		}


		public int InsertSplitter(int index, DateTime start, Color color, bool isDraggable, bool isFixed)
		{
			var found = splitters.FirstOrDefault(s => s.Index == index);
			if (found == null) return -1;
			var pos = splitters.IndexOf(found);
			if (pos < 0)
			{
				Debug.Fail("splitter index invalid!");
				return -1;
			}
			var data = new SplitterData() { BarColor = color, SplitterEndType = isDraggable ? EndType.DragEnd : EndType.LockedEnd, IsFixed = isFixed, PreciseDate = start };
			splitters.Insert(pos + 1, data);
			Rearrange();
			return data.Index;
		}

		public void RemoveSplitter(int index = -1)
		{
			var found = splitters.FirstOrDefault(s => s.Index == index);
			if (found == null) return;
			splitters.Remove(found);
			Rearrange();
		}

		private int AddSplitter(DateTime meetingStart, Color color, EndType type, bool isFixed = false)
		{
			var data = new SplitterData() { BarColor = color, SplitterEndType = type, IsFixed = isFixed, PreciseDate = meetingStart };
			splitters.Add(data);
			return data.Index;
		}

		private void Rearrange()
		{
			var fixedCnt = 0;
			var sumInterval = TimeSpan.Zero;
			if (splitters.Count < 2) return;
			var next = splitters[0];
			for (var i = 1; i < splitters.Count; i++)
			{
				var splitter = next;
				next = splitters[i];
				if (splitter.IsFixed)
					fixedCnt++;
				else
					sumInterval += next.PreciseDate - splitter.PreciseDate;
			}
			msPerPx = sumInterval.TotalMilliseconds / (pnlMain.ClientSize.Width - 2 * splitterMargin - fixedCnt * fixedGap);
			double x = splitterMargin;
			next = splitters[0];
			for (var i = 1; i <= splitters.Count; i++)
			{
				var splitter = next;
				next = i < splitters.Count ? splitters[i] : null;
				var oldX = splitter.StartX;
				splitter.StartX = (int)(x + 0.5d);
				if (splitter.IsFixed)
					x += fixedGap;
				else
					x += next != null ? (next.PreciseDate - splitter.PreciseDate).TotalMilliseconds / msPerPx : 0;
				var x1 = Math.Min(oldX, (int)x) - splitterOffset;
				var x2 = Math.Max(oldX, (int)x) + splitterOffset;
				pnlMain.Invalidate(new Rectangle(x1, 0, x2 - x1, pnlMain.ClientSize.Height));
			}
		}

		private void InvalidateSplitter(SplitterData data)
		{
			pnlMain.Invalidate(new Rectangle(data.StartX - splitterOffset, 0, splitterOffset * 2, pnlMain.ClientSize.Height));
		}

		private void PnlMainMouseMove(object sender, MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == 0) return;
			var canHovered = splitters.Where(s => s.SplitterEndType == EndType.DragEnd && s.StartX - splitterOffset <= e.X && e.X < s.StartX + splitterOffset).ToList();
			var lastHover = canHovered.Contains(dragged) ? dragged : canHovered.LastOrDefault();
			foreach (var data in splitters.Where(s => s.IsHovered && s != lastHover))
			{
				data.IsHovered = false;
				InvalidateSplitter(data);
			}
			if (lastHover != null && !lastHover.IsHovered)
			{
				lastHover.IsHovered = true;
				InvalidateSplitter(lastHover);
			}
			if (dragged == null || draggedNext == null || draggedPrev == null) return;
			var oldX = dragged.StartX;
			dragged.StartX = //restrict move here too to avoid flickering
				Clamp(dragMin, dragMax, e.X);
			var x1 = Math.Min(oldX, dragged.StartX) - splitterOffset; //inclusive
			var x2 = Math.Max(oldX, draggedNext.StartX) + splitterOffset; //exclusive
			pnlMain.Invalidate(new Rectangle(x1 - 1, 0, x2 - x1 + 2, pnlMain.ClientSize.Height));
			dragged.PreciseDate = draggedPrev.PreciseDate.AddMilliseconds((dragged.StartX - draggedPrev.StartX) * msPerPx); // exact value for proper rearranging
			var firstIntv = TimeSpan.FromMinutes(Math.Round((dragged.PreciseDate - draggedPrev.PreciseDate).TotalMinutes));
			RefreshInnerText(draggedPrev, firstIntv, dragMin, dragged.StartX);
			RefreshInnerText(dragged, draggedNext.PreciseDate - draggedPrev.PreciseDate - firstIntv, dragged.StartX, dragMax);
			RefreshLabel(dragged, GetAlignedDateTimeForDragged()); // aligned value for label text
			if (draggedNext.StartX - dragged.StartX > dragged.SplitterLabel.Width)
				RefreshLabel(draggedNext, draggedNext.PreciseDate);
			else if (draggedNext.SplitterLabel != null) draggedNext.SplitterLabel.Visible = false;
			if (dragged.StartX - draggedPrev.StartX > dragged.SplitterLabel.Width)
				RefreshLabel(draggedPrev, draggedPrev.PreciseDate);
			else if (draggedPrev.SplitterLabel != null) draggedPrev.SplitterLabel.Visible = false;
			//ShowToolTipFor(dragged);
			var alignedPreciseDate = GetAlignedDateTimeForDragged();
			if (alignedPreciseDate == lastAlignedPreciseDate) return;
			SplitterTimeChanged?.Invoke(this, new SingleValueEventArgs<Tuple<int, DateTime>>(Tuple.Create(dragged.Index, alignedPreciseDate)));
			lastAlignedPreciseDate = alignedPreciseDate;
		}

		private void RefreshInnerText(SplitterData data, TimeSpan intval, int left, int right)
		{
			var rounded = TimeSpan.FromMinutes(Math.Round(intval.TotalMinutes));
			var text = rounded.ToString("hh\\:mm");
			var size = TextRenderer.MeasureText(text, pnlMain.Font);
			if (size.Width + 15 > right - left) text = null;
			//if (data.InnerText == text) return;
			data.InnerText = text;
			pnlMain.Invalidate(new Rectangle(left, (pnlMain.ClientSize.Height - size.Height) / 2, right - left, size.Height));
		}

		private void RefreshLabel(SplitterData data, DateTime time)
		{
			if (data.SplitterLabel == null)
			{
				data.SplitterLabel = new Label(){AutoSize = true};
				pnlLabels.Controls.Add(data.SplitterLabel);
			}
			data.SplitterLabel.Text = time.ToLocalTime().ToString("t");
			data.SplitterLabel.Location = new Point(data.StartX - data.SplitterLabel.Width / 2, (pnlLabels.Height - data.SplitterLabel.Height) / 2);
			data.SplitterLabel.Visible = true;
		}

		private void PnlMainMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			if (dragged != null)
			{
				dragged.InnerText = null;
				pnlMain.Invalidate(new Rectangle(dragged.StartX, 5, draggedNext.StartX - dragged.StartX, pnlMain.ClientSize.Height - 5));
				draggedPrev.InnerText = null;
				pnlMain.Invalidate(new Rectangle(draggedPrev.StartX, 5, dragged.StartX - draggedPrev.StartX, pnlMain.ClientSize.Height - 5));
				dragged.PreciseDate = GetAlignedDateTimeForDragged();
				Rearrange();
				if (dragged.SplitterLabel != null)
				{
					pnlLabels.Controls.Remove(dragged.SplitterLabel);
					dragged.SplitterLabel = null;
				}
				if (draggedPrev.SplitterLabel != null)
				{
					pnlLabels.Controls.Remove(draggedPrev.SplitterLabel);
					draggedPrev.SplitterLabel = null;
				}
				if (draggedNext.SplitterLabel != null)
				{
					pnlLabels.Controls.Remove(draggedNext.SplitterLabel);
					draggedNext.SplitterLabel = null;
				}
				SplitterTimeChanged?.Invoke(this, new SingleValueEventArgs<Tuple<int, DateTime>>(Tuple.Create(dragged.Index, dragged.PreciseDate)));
				dragged = null;
			}
			tip.RemoveAll();
			tip.Tag = null;
		}

		private DateTime GetAlignedDateTimeForDragged()
		{
			var mins = Math.Round((dragged.StartX - draggedPrev.StartX) * msPerPx / 60000);
			if (mins < 1) mins = 1;
			else while (draggedNext.PreciseDate - draggedPrev.PreciseDate.AddMinutes(mins) < TimeSpan.FromMinutes(1)) mins--;
			var exactDate = draggedPrev.PreciseDate.AddMinutes(mins);
			return exactDate;
		}

		private static DateTime RoundDate(DateTime exactDate)
		{
			exactDate = exactDate.AddMinutes(exactDate.Second / 30);
			var roundedDate = new DateTime(exactDate.Year, exactDate.Month, exactDate.Day, exactDate.Hour, exactDate.Minute, 0);
			return roundedDate;
		}

		private void PnlMainMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			var iconTop = (pnlMain.ClientSize.Height - imgHeight) / 2;
			dragged = splitters.LastOrDefault(s => s.SplitterEndType == EndType.DragEnd && s.StartX - splitterOffset <= e.X && e.X < s.StartX + splitterOffset && iconTop <= e.Y && e.Y < iconTop + imgHeight);
			if (dragged == null)
			{
				if (splitters.Count == 0) return;
				var next = splitters[0];
				for (var i = 1; i < splitters.Count; i++)
				{
					var splitter = next;
					next = splitters[i];
					if (splitter.StartX <= e.X && e.X < next.StartX)
						IntervalSelected?.Invoke(this, new SingleValueEventArgs<int>(splitter.Index));
				}
				return;
			}
			dragPos = new Point(e.X, e.Y);
			var pos = splitters.IndexOf(dragged);
			if (pos < 1 || pos > splitters.Count - 2)
			{
				Debug.Fail("splitter index invalid!");
				return;
			}
			draggedPrev = splitters[pos - 1]; // -1, +1 indexes are exists, because there are borber (locked) anchors
			draggedNext = splitters[pos + 1];
			dragMin = draggedPrev.StartX; 
			dragMax = draggedNext.StartX;
			//ShowToolTipFor(dragged);
		}

		private void PnlMainMouseLeave(object sender, EventArgs e)
		{
			foreach (var data in splitters.Where(s => s.IsHovered))
			{
				data.IsHovered = false;
				InvalidateSplitter(data);
			}

			//tip.Hide((SplitterControl)sender);
		}

		private void SplitterMouseEnter(object sender, EventArgs e)
		{
			//ShowToolTipFor((Panel)sender);
		}

		public bool AddSplitter()
		{
			return true;
		}

		public void GetInterval(int idx, out DateTime selStartDate, out DateTime selEndDate)
		{
			selEndDate = DateTime.MaxValue;
			selStartDate = DateTime.MinValue;
		}

		private static int Clamp(int min, int max, int curr)
		{
			return curr < min
				? min
				: curr > max
					? max
					: curr;
		}

		private class SplitterData
		{
			public SplitterData()
			{
				Index = nextIndex++;
			}

			public EndType SplitterEndType { get; set; }
			public bool IsHovered { get; set; }
			public bool IsFixed { get; set; }
			public int StartX { get; set; }
			public Color BarColor { get; set; }
			public Label SplitterLabel { get; set; }
			public string InnerText { get; set; }
			public DateTime PreciseDate { get; set; }
			public int Index { get; }
		}

		public enum EndType
		{
			DragEnd,
			LockedEnd,
		}

		private class CustomControl : UserControl
		{
			public List<SplitterData> splitters;
			private Bitmap canvas;

			public CustomControl()
			{
				SetStyle(ControlStyles.UserPaint |
				         //ControlStyles.AllPaintingInWmPaint |
				         ControlStyles.DoubleBuffer,
					true);
			}

			protected override void OnSizeChanged(EventArgs e)
			{
				using (canvas) { }
				canvas = null;
				base.OnSizeChanged(e);
			}

			protected override void OnPaintBackground(PaintEventArgs e)
			{
				Rectangle clipRectangle;
				if (canvas == null)
				{
					canvas = new Bitmap(ClientSize.Width, ClientSize.Height);
					using (var g = Graphics.FromImage(canvas))
					using (var brush = new SolidBrush(BackColor))
					{
						g.FillRectangle(brush, 0, 0, canvas.Width, canvas.Height);
					}
					clipRectangle = ClientRectangle;
				}
				else
				{
					clipRectangle = e.ClipRectangle;
				}
				if (splitters != null && splitters.Count > 0)
				{
					using (var g = Graphics.FromImage(canvas))
					{
						if (splitters.Count < 1) return;
						var next = splitters[0];
						for (var i = 1; i < splitters.Count; i++)
						{
							var splitter = next;
							next = splitters[i];
							var barRect = new Rectangle(splitter.StartX, 5, next.StartX - splitter.StartX, ClientSize.Height - 5);
							barRect.Intersect(clipRectangle);
							if (barRect.IsEmpty) continue;
							using (var brush = new SolidBrush(splitter.BarColor))
							{
								g.FillRectangle(brush, barRect);
							}
							if (!string.IsNullOrEmpty(splitter.InnerText))
							{
								var clcp = (byte)(splitter.BarColor.GetBrightness() * 256 + 128);
								using (var brush = new SolidBrush(Color.FromArgb(clcp, clcp, clcp)))
								{
									g.DrawString(splitter.InnerText, Font, brush, new Rectangle(splitter.StartX, 5, next.StartX - splitter.StartX, ClientSize.Height - 10), new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center});
								}
							}
						}
						foreach (var splitter in splitters.Where(s => s.SplitterEndType == EndType.LockedEnd).Concat(splitters.Where(s => s.SplitterEndType == EndType.DragEnd && !s.IsHovered)).Concat(splitters.Where(s => s.SplitterEndType == EndType.DragEnd && s.IsHovered)))
						{
							var idx = splitters.IndexOf(splitter);
							var prevX = idx > 0 ? splitters[idx - 1].StartX : splitterMargin;
							var nextX = idx < splitters.Count - 1 ? splitters[idx + 1].StartX : ClientSize.Width - splitterMargin;
							var upperRect = new Rectangle(prevX + 1, 0, nextX - prevX - 2, 5);
							upperRect.Intersect(clipRectangle);
							var lowerRect = new Rectangle(prevX + 1, ClientSize.Height - 5, nextX - prevX - 2, ClientSize.Height);
							lowerRect.Intersect(clipRectangle);
							if (!upperRect.IsEmpty || !lowerRect.IsEmpty)
							{
								using (var brush = new SolidBrush(BackColor))
								{
									g.FillRectangles(brush, new[] {upperRect, lowerRect});
								}
							}
							if (!new Rectangle(splitter.StartX - splitterOffset, 0, splitterOffset * 2, ClientSize.Height).IntersectsWith(clipRectangle)) continue;
							using (var pen = new Pen(splitter.IsHovered ? hoverColor : caretColor))
							{
								g.DrawRectangle(pen, splitter.StartX - 1, 0, 1, ClientSize.Height);
							}
							var img = splitter.SplitterEndType == EndType.LockedEnd ? Resources.locked_end : splitter.IsHovered ? Resources.drag_end_hover : Resources.drag_end;
							g.DrawImage(img, splitter.StartX - splitterOffset, (ClientSize.Height - img.Height) / 2, img.Width, img.Height);
						}
					}
				}
				using (var g = e.Graphics)
				{
					g.DrawImage(canvas, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
				}
			}

			protected override void Dispose(bool disposing)
			{
				using (canvas) { } 
				base.Dispose(disposing);
			}
		}
	}

}
