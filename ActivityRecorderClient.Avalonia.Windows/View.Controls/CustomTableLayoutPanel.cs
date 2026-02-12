using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed partial class CustomTableLayoutPanel : UserControl
	{
		private int currentHeight = 0;
		private int customControls = 0;
		private int lastFocusedIndex = -1;

		public int Count
		{
			get { return customControls; }
		}

		public CustomTableLayoutPanel()
		{
			InitializeComponent();
			DoubleBuffered = true;
		}

		public void AddControl(WorkRowBase c, bool setHeight = true)
		{
			if (customControls != 0)
			{
				var splitter = new SmallSplitter
				{
					Location = new Point(0, currentHeight),
					Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
					Width = Width,
					TabStop = false
				};
				Controls.Add(splitter);
				splitter.GotFocus += ControlGotFocus;
				currentHeight += splitter.Height;
			}

			c.Location = new Point(0, currentHeight);
			c.Width = this.Width;
			currentHeight += c.Height;
			if (setHeight)
			{
				Height = currentHeight;
			}

			Controls.Add(c);
			c.GotFocus += ControlGotFocus;
			customControls++;
		}

		private void ControlGotFocus(object sender, EventArgs e)
		{
			if (!(sender is Control control)) return;
			var idx = Controls.IndexOf(control);
			if (sender is SmallSplitter)
			{
				if (idx > lastFocusedIndex)
				{
					if (idx + 1 < customControls)
						Controls[idx + 1].Select();
				}
				else
				{
					if (idx - 1 >= 0)
						Controls[idx - 1].Select();
				}
				return;
			}
			lastFocusedIndex = idx;
		}

		public void ClearControls()
		{
			Visible = false;
			Height = 0;
			customControls = 0;
			currentHeight = 0;
			while (Controls.Count > 0)
			{
				Controls[0].GotFocus -= ControlGotFocus;
				Controls[0].Dispose();
			}

			Visible = true;
		}

		public IEnumerable<WorkRowBase> GetControls()
		{
			for (int i = 0; i < Controls.Count; i += 2)
			{
				Debug.Assert(Controls[i] is WorkRowBase);
				yield return (WorkRowBase) Controls[i];
			}
		}

		public void SetHeight()
		{
			Height = currentHeight;
		}

		public void Trim(int size)
		{
			if (size >= customControls) return;
			if (size == 0)
			{
				ClearControls();
				return;
			}

			for (int i = 0; i < customControls - size; ++i)
			{
				var ctrl = Controls[Controls.Count - 1];
				currentHeight -= ctrl.Height;
				ctrl.Dispose();
				var splitter = Controls[Controls.Count - 1];
				currentHeight -= splitter.Height;
				splitter.Dispose();
			}

			customControls = size;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			foreach (Control control in Controls)
			{
				control.Width = Width;
			}
		}
	}
}