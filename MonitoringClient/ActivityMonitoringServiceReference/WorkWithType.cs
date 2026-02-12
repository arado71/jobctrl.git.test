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
	public partial class WorkWithType : IEquatable<WorkWithType>
	{
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(obj, null))
				return false;

			if (obj is WorkWithType)
			{
				return this.Equals((WorkWithType)obj);
			}
			return false;
		}

		public bool Equals(WorkWithType other)
		{
			if (Object.ReferenceEquals(other, null))
				return false;

			return this.Type == other.Type && this.WorkId == other.WorkId;
		}

		public override int GetHashCode()
		{
			int result = 17;
			result = 31 * result + Type.GetHashCode();
			result = 31 * result + WorkId.GetHashCode();
			return result;
		}
	}
}
