using System;
using Tct.ActivityRecorderClient.Hotkeys;

namespace Tct.ActivityRecorderClient.Capturing.UserActivity
{
	public interface IUserActivityService : IDisposable
	{
		void Start();
		void Stop();
		void GetAndResetCounters(out int keyboardActivity, out int mouseActivity);
		int? GetLastActivity();
		int? GetLastMouseActivityTime();
		int? GetLastKeyboardActivityTime();
	}
}
