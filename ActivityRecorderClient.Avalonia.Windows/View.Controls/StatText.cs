using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Tct.ActivityRecorderClient.Properties;
using MetroFramework;

namespace Tct.ActivityRecorderClient.View.Controls
{
    public partial class StatText : UserControl, ILocalizableControl
    {
        public event EventHandler StatsClicked;

		public void SetText(string lastQueryTime)
		{
			if (lastQueryTime == null) timeLabel.Visible = false;
			else
			{
				timeLabel.Text = lastQueryTime;
				timeLabel.Visible = true;
			}
		}

		public void setProgressSpinner(bool visible)
		{
			metroProgressSpinner.Visible = visible;
		}

		public StatText()
        {
            InitializeComponent();
        }

        public void Localize()
        {
            metroLink1.Text = Labels.Worktime_Tooltip;
            var textSize = TextRenderer.MeasureText(metroLink1.Text, MetroFonts.Link(metroLink1.FontSize, metroLink1.FontWeight));
            metroLink1.Width = textSize.Width;
        }

        private void StatText_Load(object sender, EventArgs e)
        {
            metroLink1.Text = Labels.Worktime_Tooltip;
            var textSize = TextRenderer.MeasureText(metroLink1.Text, MetroFonts.Link(metroLink1.FontSize, metroLink1.FontWeight));
            metroLink1.Width = textSize.Width;
        }

        private void StatText_Click(object sender, EventArgs e)
        {
            StatsClicked?.Invoke(this, EventArgs.Empty);
        }

        private void StatText_MouseEnter(object sender, EventArgs e)
        {
            pStatsBtn.BackgroundImage = Resources.stats_grey;
            metroLink1.ForeColor = Color.FromArgb(128, 128, 128);
        }

        private void StatText_MouseLeave(object sender, EventArgs e)
        {
            pStatsBtn.BackgroundImage = Resources.stats_blue;
            metroLink1.ForeColor = Color.FromArgb(0, 174, 219);
        }
    }
}
