using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Meeting
{
	public interface IAddressBookService : IDisposable
	{
		void Initialize();
		bool IsAddressBookServiceAvailable { get; }
		List<MeetingAttendee> DisplaySelectNamesDialog(IntPtr parentWindowHandle);
	}
}
