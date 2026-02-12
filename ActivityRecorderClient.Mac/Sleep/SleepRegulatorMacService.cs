using System;
using System.Runtime.InteropServices;
using log4net;
using MonoMac.CoreFoundation;

namespace Tct.ActivityRecorderClient.Sleep
{
	//https://developer.apple.com/library/mac/#qa/qa2004/qa1340.html
	//http://www.opensource.apple.com/source/IOKitUser/IOKitUser-502/pwr_mgt.subproj/IOPMLib.h
	//https://developer.apple.com/library/mac/#documentation/Darwin/Reference/IOKit/IOPMLib_h/index.html
	public class SleepRegulatorMacService : ISleepRegulatorService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly CFString kIOPMAssertionTypeNoIdleSleep = new CFString("NoIdleSleepAssertion");
		private static readonly CFString activityReason = new CFString("Working with JobCTRL");
		private int? assertionId;

		public void PreventSleep()
		{
			int aid;
			var res = IOPMAssertionCreateWithName(kIOPMAssertionTypeNoIdleSleep.Handle, IOPMAssertionLevel.On, activityReason.Handle, out aid);
			if (res == IOReturn.Success)
			{
				assertionId = aid;
				log.Debug("Preventing sleep");
			}
			else
			{
				log.Warn("Unable to prevent sleep");
			}
		}

		public void AllowSleep()
		{
			if (assertionId.HasValue)
			{
				var res = IOPMAssertionRelease(assertionId.Value);
				if (res == IOReturn.Success)
				{
					assertionId = null;
					log.Debug("Allowing sleep");
				}
				else
				{
					log.Warn("Unable to allow sleep");
				}
			}
			else
			{
				log.Warn("Nothing to allow");
			}
		}


		private enum IOPMAssertionLevel
		{
			Off = 0,
			On = 255,
		}

		private enum IOReturn
		{
			Success = 0,
		}

		[DllImport(LibraryMac.IOKit.Path)]
		private static extern IOReturn IOPMAssertionCreateWithName(IntPtr assertionType, IOPMAssertionLevel mIOPMAssertionLevel, IntPtr reasonForActivity, out int assertionId);

		[DllImport(LibraryMac.IOKit.Path)]
		private static extern IOReturn IOPMAssertionRelease(int assertionId);
	}
}

