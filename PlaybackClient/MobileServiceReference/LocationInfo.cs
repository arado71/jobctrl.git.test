using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient.MobileServiceReference
{
	partial class LocationInfo
	{
		public DateTime DateTyped
		{
			get { return WorkItem.DateTimeFromString(Date); }
			set { Date = WorkItem.DateTimeToString(value); }
		}

		public override string ToString()
		{
			return "mobileLocation workId: " + WorkId + " lat: " + Latitude + " lon: " + Longitude + " acc: " + Accuracy + " date: " + DateTyped;
		}
	}
}
