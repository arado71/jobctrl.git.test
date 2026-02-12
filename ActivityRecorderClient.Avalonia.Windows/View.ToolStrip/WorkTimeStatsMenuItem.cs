using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class WorkTimeStatsMenuItem : ToolStripControlHost
	{
		public WorkTimeStatsControl WorkTimeStatsControl { get; private set; }

		public WorkTimeStatsMenuItem()
			: base(new WorkTimeStatsControl())
		{
			WorkTimeStatsControl = Control as WorkTimeStatsControl;
			AutoSize = false;
			Margin = new Padding(0);
			Padding = new Padding(0);
		}
	}
}
