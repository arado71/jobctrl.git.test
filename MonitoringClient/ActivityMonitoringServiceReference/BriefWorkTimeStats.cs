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
	public partial class BriefWorkTimeStats
	{
		public TimeSpan SumWorkTime
		{
			get { return ComputerWorkTime + IvrWorkTime + MobileWorkTime + ManuallyAddedWorkTime + HolidayTime + SickLeaveTime; }
		}

		public void Update(BriefWorkTimeStats newStats)
		{
			var sumChanged = this.SumWorkTime != newStats.SumWorkTime;
			this.ComputerWorkTime = newStats.ComputerWorkTime;
			this.HolidayTime = newStats.HolidayTime;
			this.IvrWorkTime = newStats.IvrWorkTime;
			this.ManuallyAddedWorkTime = newStats.ManuallyAddedWorkTime;
			this.MobileWorkTime = newStats.MobileWorkTime;
			this.SickLeaveTime = newStats.SickLeaveTime;
			if (sumChanged) this.RaisePropertyChanged("SumWorkTime");
		}
	}
}
