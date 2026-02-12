using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JobCTRL.Plugins;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	//quick and dirty sta shared wrapper
	public class StaSharedCaptureExtensionWrapper<T> : ICaptureExtension, IDisposable where T : class, ICaptureExtension
	{
		// ReSharper disable StaticFieldInGenericType
		private static readonly object sharedLock = new object();
		private static int instanceCount;
		private static SharedStaWarpper instance;
		// ReSharper restore StaticFieldInGenericType

		private readonly T plugin;

		public StaSharedCaptureExtensionWrapper(Func<T> factoryFunc)
		{
			plugin = factoryFunc();
			if (plugin == null) throw new InvalidOperationException();
			lock (sharedLock)
			{
				var first = instanceCount++ == 0;
				if (first)
				{
					instance = new SharedStaWarpper();
				}
			}
		}

		public void Dispose()
		{
			lock (sharedLock)
			{
				var last = --instanceCount == 0;
				if (last)
				{
					instance.Dispose();
					instance = null;
				}
			}
		}

		public string Id
		{
			get
			{
				using (instance.UseAs(plugin))
				{
					return instance.Id;
				}
			}
		}

		public IEnumerable<string> GetParameterNames()
		{
			using (instance.UseAs(plugin))
			{
				return instance.GetParameterNames();
			}
		}

		public void SetParameter(string name, string value)
		{
			using (instance.UseAs(plugin))
			{
				instance.SetParameter(name, value);
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			using (instance.UseAs(plugin))
			{
				return instance.GetCapturableKeys();
			}
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			using (instance.UseAs(plugin))
			{
				return instance.Capture(hWnd, processId, processName);
			}
		}

		private class SharedStaWarpper : ICaptureExtension, IDisposable
		{
			private readonly object thisLock = new object();
			private readonly CaptureExtensionWrapper wapper = new CaptureExtensionWrapper();
			private readonly StaCaptureExtensionWrapper staCaptureExtension;

			public SharedStaWarpper()
			{
				staCaptureExtension = new StaCaptureExtensionWrapper(() => wapper);
			}

			public IDisposable UseAs(ICaptureExtension inner)
			{
				return new InnerState(this, inner);
			}

			public void Dispose()
			{
				staCaptureExtension.Dispose();
			}

			public string Id
			{
				get { return staCaptureExtension.Id; }
			}

			public IEnumerable<string> GetParameterNames()
			{
				return staCaptureExtension.GetParameterNames();
			}

			public void SetParameter(string name, string value)
			{
				staCaptureExtension.SetParameter(name, value);
			}

			public IEnumerable<string> GetCapturableKeys()
			{
				return staCaptureExtension.GetCapturableKeys();
			}

			public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
			{
				return staCaptureExtension.Capture(hWnd, processId, processName);
			}

			private class InnerState : IDisposable
			{
				private readonly ICaptureExtension oldInner;
				private readonly SharedStaWarpper parent;

				public InnerState(SharedStaWarpper parent, ICaptureExtension inner)
				{
					this.parent = parent;
					if (parent == null || parent.wapper == null) throw new InvalidOperationException();
					Monitor.Enter(parent.thisLock);
					oldInner = parent.wapper.Inner;
					parent.wapper.Inner = inner;
				}

				public void Dispose()
				{
					parent.wapper.Inner = oldInner;
					Monitor.Exit(parent.thisLock);
				}
			}

			private class CaptureExtensionWrapper : ICaptureExtension
			{
				public ICaptureExtension Inner { get; set; }

				public string Id
				{
					get { return Inner.Id; }
				}

				public IEnumerable<string> GetParameterNames()
				{
					return Inner.GetParameterNames();
				}

				public void SetParameter(string name, string value)
				{
					Inner.SetParameter(name, value);
				}

				public IEnumerable<string> GetCapturableKeys()
				{
					return Inner.GetCapturableKeys();
				}

				public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
				{
					return Inner.Capture(hWnd, processId, processName);
				}
			}
		}
	}
}
