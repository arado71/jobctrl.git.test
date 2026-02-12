using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Marshalls calls to spawned STA thread, where only one call can run at a time.
	/// If a call doesn't return in time (timeout), or a previous call is still running, the invokation fails.
	/// </summary>
	public class StaTask : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly AutoResetEvent startProcessing = new AutoResetEvent(false);
		private readonly object thisLock = new object();
		private Func<object> processingFunc;
		private bool isRunning;
		private bool isBusy;

		private Type currentRoundType;
		private volatile bool currentRoundHasResult;
		private volatile object currentRoundResult;

		private volatile bool isAwaiting;

		public event EventHandler<SingleValueEventArgs<object>> ProcessingCompleted;

		public Thread Thread { get; private set; }

		// todo This field is not thread-safe
		public TimeSpan StopTimeout { get; set; }

		public StaTask(string name = null)
		{
			StopTimeout = TimeSpan.FromMinutes(1);
			Thread = new Thread(ThreadLoop);
			Thread.SetApartmentState(ApartmentState.STA);
			Thread.IsBackground = true;
			if (!string.IsNullOrEmpty(name))
			{
				Thread.Name = name;
			}

			Thread.Start();
		}

		public void Stop(TimeSpan timeout)
		{
			lock (thisLock)
			{
				if (!isRunning) return;
				isAwaiting = false;
				isRunning = false;
				startProcessing.Set();
			}

			if (!Thread.Join(timeout))
			{
				Thread.Interrupt();
			}

			lock (thisLock)
			{
				Thread = null;
			}
		}

		public bool BeginInvoke<TResult>(Func<TResult> task)
		{
			lock (thisLock)
			{
				if (Thread == null) throw new InvalidOperationException("Thread stopped");
				if (!isRunning || isBusy) return false;
				currentRoundType = typeof(TResult);
				processingFunc = () => task();
				startProcessing.Set();
			}

			return true;
		}

		public bool TryEndInvoke<TResult>(out TResult result)
		{
			lock (thisLock)
			{
				isAwaiting = false;
				if (currentRoundType != typeof(TResult)) throw new ArgumentException("Type of TResult is invalid");
				if (currentRoundHasResult)
				{
					result = (TResult) currentRoundResult;
				}
				else
				{
					result = default(TResult);
				}

				return currentRoundHasResult;
			}
		}

		public TResult EndInvokeOrDefault<TResult>()
		{
			TResult result;
			TryEndInvoke(out result);
			return result;
		}

		public bool TryInvoke<TResult>(Func<TResult> func, out TResult result, TimeSpan timeout)
		{
			var signaller = new AutoResetEvent(false);
			EventHandler<SingleValueEventArgs<object>> onProcessed = (o, e) => signaller.Set();
			ProcessingCompleted += onProcessed;
			var couldStart = BeginInvoke(() => (object) func());
			if (couldStart)
			{
				signaller.WaitOne(timeout);
			}

			var isSuccess = TryEndInvoke(out result);
			ProcessingCompleted -= onProcessed;
			signaller.Close();
			return isSuccess;
		}

		public TResult InvokeOrDefault<TResult>(Func<TResult> func, TimeSpan timeout)
		{
			TResult result;
			TryInvoke(func, out result, timeout);
			return result;
		}

		private void ThreadLoop(object thread)
		{
			isRunning = true;
			log.Debug("Thread started");
			try
			{
				while (true)
				{
					startProcessing.WaitOne();

					lock (thisLock)
					{
						if (!isRunning) break;
						isBusy = true;
						isAwaiting = true;
						currentRoundHasResult = false;
					}

					try
					{
						var result = processingFunc();
						bool eventRequired;
						lock (thisLock)
						{
							if (!isRunning) break;
							currentRoundHasResult = true;
							currentRoundResult = result;
							eventRequired = isAwaiting;
						}
						if (eventRequired)
						{
							OnProcessingCompleted(result);
						}
					}
					catch (Exception ex)
					{
						log.Debug("Unhandled exception inside STA thread processing", ex);
					}

					lock (thisLock)
					{
						if (!isRunning) break;
						isAwaiting = false;
						isBusy = false;
						startProcessing.Reset();
					}
				}

				log.Verbose("Stopping thread");
			}
			catch (Exception ex)
			{
				log.Error("STA thread failed", ex);
			}
		}

		private void OnProcessingCompleted(object result)
		{
			var evt = ProcessingCompleted;
			if (evt != null) evt(this, new SingleValueEventArgs<object>(result));
		}

		public void Dispose()
		{
			Stop(StopTimeout);
			startProcessing.Close();
		}
	}
}
