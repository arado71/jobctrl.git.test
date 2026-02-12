using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	public static class TaskEx
	{
		public static Task Delay(TimeSpan amount)
		{
			return Delay((int)amount.TotalMilliseconds);
		}

		public static Task Delay(int milliseconds)
		{
			if (milliseconds < 0)
			{
				throw new ArgumentOutOfRangeException("milliseconds");
			}

			var taskCompletionSource = new TaskCompletionSource<object>();

			//this ctor keeps the timer alive (this is crucial)! //http://msdn.microsoft.com/en-us/library/ms149618.aspx
			var timer = new Timer(self =>
			{
				try
				{
					taskCompletionSource.SetResult(null);
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
				}
				finally
				{
					((Timer)self).Dispose();
				}
			});
			timer.Change(milliseconds, Timeout.Infinite);

			return taskCompletionSource.Task;
		}
	}
}
