using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient.ActivityMonitoringServiceReference
{
	public partial class BriefWorkStats
	{
		public void Update(BriefWorkStats newStats)
		{
			this.WorkId = newStats.WorkId;
			this.WorkName = newStats.WorkName;
			this.WorkTimeStats.Update(newStats.WorkTimeStats);
		}
	}
}
