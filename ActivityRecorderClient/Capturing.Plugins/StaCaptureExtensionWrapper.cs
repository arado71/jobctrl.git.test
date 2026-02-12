using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JobCTRL.Plugins;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	//quick and dirty sta wrapper
	public class StaCaptureExtensionWrapper : ICaptureExtension, IDisposable
	{
		private readonly ICaptureExtension inner;
		private readonly Thread staThread;
		private readonly BlockingQueue<ExecutableUserWorkItem> workItems = new BlockingQueue<ExecutableUserWorkItem>();

		public StaCaptureExtensionWrapper(Func<ICaptureExtension> factoryFunc)
		{
			if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");
			inner = factoryFunc();
			if (inner == null) throw new InvalidOperationException("factoryFunc returned null");
			staThread = new Thread(ThreadLoop);
			staThread.SetApartmentState(ApartmentState.STA);
			staThread.IsBackground = true;
			staThread.Start();
		}

		private void ThreadLoop(object state)
		{
			while (Interlocked.CompareExchange(ref isDisposed, 0, 0) == 0)
			{
				var item = workItems.Dequeue();
				if (item == null) continue;
				lock (item.ThisLock)
				{
					try
					{
						item.Execute();
					}
					catch (Exception ex)
					{
						item.SetException(ex);
					}
					Monitor.Pulse(item.ThisLock); //signal we are ready
				}
			}
		}

		private void Send(Action action)
		{
			var item = new UserWorkItem(action);
			lock (item.ThisLock)
			{
				workItems.Enqueue(item);
				Monitor.Wait(item.ThisLock); //wait for execution
				item.EnsureNoException();
			}
		}

		private T Send<T>(Func<T> func)
		{
			var item = new UserWorkItem<T>(func);
			lock (item.ThisLock)
			{
				workItems.Enqueue(item);
				Monitor.Wait(item.ThisLock); //wait for execution
				item.EnsureNoException();
				return item.Result;
			}
		}

		public string Id
		{
			get { return Send(() => inner.Id); }
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Send(() => inner.GetParameterNames());
		}

		public void SetParameter(string name, string value)
		{
			Send(() => inner.SetParameter(name, value));
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			return Send(() => inner.GetCapturableKeys());
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			return Send(() => inner.Capture(hWnd, processId, processName));
		}

		private int isDisposed;
		public void Dispose()
		{
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			workItems.Enqueue(null); //signal stop
			using (inner as IDisposable) { } //dispose inner
			staThread.Join();
		}

		private abstract class ExecutableUserWorkItem
		{
			public readonly object ThisLock = new object(); //for signaling (no ManualResetEventSlim...)
			public abstract void Execute();

			private Exception exception;
			public void SetException(Exception ex)
			{
				exception = ex;
			}

			public void EnsureNoException()
			{
				if (exception != null) throw new Exception("Error is rethrown", exception);
			}
		}

		private class UserWorkItem : ExecutableUserWorkItem
		{
			private readonly Action exec;

			public UserWorkItem(Action action)
			{
				exec = action;
			}

			public override void Execute()
			{
				exec();
			}
		}

		private class UserWorkItem<T> : ExecutableUserWorkItem
		{
			public T Result;
			private readonly Func<T> exec;

			public UserWorkItem(Func<T> func)
			{
				exec = func;
			}

			public override void Execute()
			{
				Result = exec();
			}
		}
	}
}
