using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Model.Mobile
{
	public class MobilePhoneCall
	{
		public int UserId { get; set; }
		public string PhoneNumber { get; set; }
		public bool IsInbound { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public TimeSpan GetDuration()
		{
			return EndDate - StartDate;
		}

		public static readonly IComparer<MobilePhoneCall> StartDateComparer = new MobilePhoneCallStartDateComparer();

		private class MobilePhoneCallStartDateComparer : IComparer<MobilePhoneCall>
		{
			public int Compare(MobilePhoneCall x, MobilePhoneCall y)
			{
				return DateTime.Compare(x.StartDate, y.StartDate);
			}
		}
	}
}
