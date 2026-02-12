using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient.MobileServiceReference
{
	partial class UploadData_v4Request
	{
		public DateTime ScheduledTime
		{
			get
			{
				return WorkItems != null && WorkItems.Count > 0
					? WorkItems.Max(n => n.EndDateTyped)
					: LocationInfos != null && LocationInfos.Count > 0
						? LocationInfos.Max(n => n.DateTyped)
						: DateTime.MinValue;
			}
		}

		public override string ToString()
		{
			return "MobileRequest userId: " + UserId + " " + (WorkItems == null ? "" : string.Join(", ", WorkItems)) + " " + (LocationInfos == null ? "" : string.Join(", ", LocationInfos));
		}
	}
}
