using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MonitoringClient.ActivityMonitoringServiceReference;

namespace MonitoringClient.Converters
{
	public class BriefUserStatsCurrentWorksConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var stats = value as BriefUserStats;
			if (stats == null || stats.CurrentWorks == null || stats.CurrentWorks.Count == 0) return "";
			return string.Join(", ", stats.CurrentWorks.Select(n => GetWorkName(n.WorkId, stats.TodaysWorksByWorkId) + " [" + n.Type + "]"));
		}

		private static string GetWorkName(int id, Dictionary<int, BriefWorkStats> works)
		{
			BriefWorkStats workStats;
			if (works != null && works.TryGetValue(id, out workStats) && workStats != null)
			{
				return workStats.WorkName + " (" + workStats.WorkId + ")";
			}
			return "Unknown work (" + id + ")";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string strValue = value as string;
			//yagni
			return DependencyProperty.UnsetValue;
		}
	}
}
