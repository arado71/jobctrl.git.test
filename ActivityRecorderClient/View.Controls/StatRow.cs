using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed partial class StatRow : UserControl
	{
		private bool deltaNegative;
		private string deltaValue;
		private bool sumDeltaVisible;

		public string Title
		{
			get
			{
				return lblTitle.Text; }
			set
			{
				lblTitle.Text = value;
			}
		}

		public string LeftValue
		{
			get
			{
				return lblLeft.Text; }

			set
			{
				lblLeft.Text = value;
			}
		}

		public string RightValue
		{
			get { return lblRight.Text; }

			set
			{
				lblRight.Text = value;
			}
		}

		public string DeltaValue
		{
			get { return deltaValue; }

			set
			{
				deltaValue = value;
				lblDelta.Text = string.Format("∆ {0}", value);
				lblDelta.Visible = !string.IsNullOrEmpty(deltaValue) && sumDeltaVisible;
			}
		}

		public bool SumDeltaVisible
		{
			get { return sumDeltaVisible; }

			set
			{
				sumDeltaVisible = value;
				lblDelta.Visible = !string.IsNullOrEmpty(deltaValue) && sumDeltaVisible;
				lblRight.Visible = sumDeltaVisible;
			}
		}

		public bool DeltaNegative
		{
			get { return deltaNegative; }

			set
			{
				deltaNegative = value;
				
			}
		}

		public StatRow()
		{
			DoubleBuffered = true;
			InitializeComponent();
			lblLeft.ForeColor = StyleUtils.Foreground;
			lblRight.ForeColor = StyleUtils.ForegroundLight;
			lblTitle.ForeColor = StyleUtils.Foreground;
			lblDelta.Font = StyleUtils.GetFont(FontStyle.Bold, 8.0f);
			lblLeft.Font = StyleUtils.GetFont(FontStyle.Bold, 8.0f);
			lblRight.Font = StyleUtils.GetFont(FontStyle.Bold, 8.0f);
			lblTitle.Font = StyleUtils.GetFont(FontStyle.Regular, 8.0f);
		}
	}
}