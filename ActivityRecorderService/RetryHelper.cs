using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService
{
	public static class RetryHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static async Task<bool> RetryAtEvenIntervalsAsync(Func<Task<bool>> func, int retries, int maxWait, string funcName = null)
		{
			var rnd = new Random();
			Exception lastException = null;
			bool result = true;
			var sw = new Stopwatch();
			do
			{
				if (!result)
				{
					LogResult(funcName, RetryResult.IntermediateError, lastException, sw, retries);
					await Task.Delay(rnd.Next(maxWait / 2, maxWait));
				}
				sw.Reset(); sw.Start();
				try
				{
					result = await func();
				}
				catch (Exception ex)
				{
					result = false;
					lastException = ex;
				}
				finally
				{
					sw.Stop();
				}
			} while (!result && --retries >= 0);
			if (result)
			{
				LogResult(null, RetryResult.FinalOk, null, sw); //don't log happy path
			}
			else
			{
				LogResult(funcName, RetryResult.FinalError, lastException, sw);
			}
			return result;
		}

		public static bool RetryAtEvenIntervals(Func<bool> func, int retries, int wait)
		{
			Exception _;
			return RetryAtEvenIntervals(func, retries, wait, null, out _);
		}

		public static bool RetryAtEvenIntervals(Func<bool> func, int retries, int wait, string funcName, out Exception lastException)
		{
			lastException = null;
			bool result = true;
			var sw = new Stopwatch();
			do
			{
				if (!result)
				{
					LogResult(funcName, RetryResult.IntermediateError, lastException, sw, retries);
					Thread.Sleep(wait);
				}
				sw.Reset(); sw.Start();
				try
				{
					result = func();
					lastException = null;
				}
				catch (Exception ex)
				{
					result = false;
					lastException = ex;
				}
				finally
				{
					sw.Stop();
				}
			} while (!result && --retries >= 0);
			if (result)
			{
				LogResult(funcName, RetryResult.FinalOk, lastException, sw);
			}
			else
			{
				LogResult(funcName, RetryResult.FinalError, lastException, sw);
			}
			return result;
		}

		public static void RetryAtEvenIntervalsOnBackgroundThread(Func<bool> func, int retries, int wait, string funcName)
		{
			Exception ex;
			if (retries <= 0)
			{
				RetryAtEvenIntervals(func, 0, wait, funcName, out ex);
			}
			else
			{
				//todo remove this null funcName hax (refactor)
				//todo use a timer instead of a sleep
				var sw = Stopwatch.StartNew();
				bool result = RetryAtEvenIntervals(func, 0, wait, null, out ex);
				sw.Stop();
				if (result)
				{
					LogResult(funcName, RetryResult.FinalOk, ex, sw);
				}
				else
				{
					//if we fail on the main thread, retry on bg thread
					LogResult(funcName, RetryResult.IntermediateError, ex, sw, retries - 1);
					ThreadPool.QueueUserWorkItem(_ =>
						{
							Thread.Sleep(wait);
							Exception excBg;
							RetryAtEvenIntervals(func, --retries, wait, funcName, out excBg);
						});
				}
			}
		}

		private static void LogResult(string funcName, RetryResult result, Exception ex, Stopwatch sw, int? retries = null)
		{
			if (funcName == null) return;
			switch (result)
			{
				case RetryResult.FinalOk:
					log.Info("Successfully executed " + funcName + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
					break;
				case RetryResult.FinalError:
					log.Error("Error executing " + funcName + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ", ex);
					break;
				case RetryResult.IntermediateError:
					log.Debug("Error executing " + funcName + (retries.HasValue ? " (" + (retries.Value + 1) + " to go)" : "") + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ", ex);
					break;
				default:
					log.Error("Unknown result " + funcName + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ", ex);
					break;
			}
		}

		private enum RetryResult
		{
			FinalOk = 0,
			FinalError = 1,
			IntermediateError = 2,
		}
	}
}
