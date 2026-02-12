using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public sealed partial class WorkRowCompact : WorkRowBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public WorkRowCompact(NavigationBase navigation)
		{
			InitializeComponent();
			SetColorScheme();
			DoubleBuffered = true;
			Navigation = navigation;
		}

		public override void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				BackColor = SystemColors.Window;
				lblText.ForeColor = SystemColors.WindowText;
				lblText.ForeColorAlternative = SystemColors.HighlightText;
				lblText.BackColor = SystemColors.Window;
			}
			else
			{
				BackColor = StyleUtils.Background;
				lblText.ForeColor = StyleUtils.ForegroundDark;
				lblText.ForeColorAlternative = StyleUtils.Foreground;
				lblText.BackColor = StyleUtils.Background;
			}
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			return new Size(lblText.Location.X + lblText.GetPreferredSize(proposedSize).Width, Height);
		}

		public int GetPreferredWidth()
		{
			return lblText.Location.X + lblText.PreferredWidth;
		}

		protected override void OnIdChanged()
		{
			workIcon1.Color = StyleUtils.GetColor(navigation.Id);
		}

		protected override void OnNameChanged()
		{
			RenderValue();
		}

		protected override void OnPathChanged()
		{
			RenderValue();
		}

		protected override void RenderSelection()
		{
			lblText.BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			lblText.RenderText();
		}

		// todo possible double rendering b/o Path,Name change
		protected override void RenderValue()
		{
			int w = lblText.Width;
			lblText.Width = 2048; // hax Make sure it fits in one line
			lblText.Clear();
			if (navigation.Path != null && navigation.Path.Any())
			{
				lblText.AddColorChange()
					.AddText(string.Join(WorkDataWithParentNames.DefaultSeparator, navigation.Path.ToArray()))
					.AddText(WorkDataWithParentNames.DefaultSeparator)
					.AddColorChange();
			}

			lblText.AddText(navigation.Name).RenderText();
			lblText.Width = w;
			AccessibleName = navigation.Name;
		}

		private void HandleClicked(object sender, MouseEventArgs e)
		{
			log.DebugFormat("UI - Element clicked with {0}", e.Button);
			TelemetryHelper.RecordFeature("Search", "StartMouse");
			navigation.Navigate();
			Selected = false;
			OnClick(e);
		}

		private void HandleMouseEntered(object sender, EventArgs e)
		{
			Selected = true;
		}
	}
}