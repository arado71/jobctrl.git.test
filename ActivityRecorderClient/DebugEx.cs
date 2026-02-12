using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Diagnostics
{
	public static class DebugEx
	{
		[Conditional("DEBUG")]
		public static void EnsureSta()
		{
			Debug.Assert(System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA, "Method not called from an STA thread");
		}

		private static int? guiThreadId;
		private static bool allowThreadAsserts = true;

		[Conditional("DEBUG")]
		public static void SetGuiThread()
		{
			Debug.Assert(!guiThreadId.HasValue, "Duplicate Set");
			guiThreadId = Thread.CurrentThread.ManagedThreadId;
		}

#if DEBUG
		public static IDisposable DisableThreadAsserts()
		{
			return new DebugThreadDisable();
		}
#endif

		public static bool IsInDesignMode
		{
			get
			{
				bool isInDesignMode = ComponentModel.LicenseManager.UsageMode == ComponentModel.LicenseUsageMode.Designtime || Debugger.IsAttached == true;

				if (!isInDesignMode)
				{
					using (var process = Process.GetCurrentProcess())
					{
						return process.ProcessName.ToLowerInvariant().Contains("devenv");
					}
				}

				return isInDesignMode;
			}
		}

		[Conditional("DEBUG")]
		public static void EnsureGuiThread()
		{
			if (!allowThreadAsserts || IsInDesignMode) return;
			Debug.Assert(guiThreadId.HasValue, "No Set was called");
			Debug.Assert(guiThreadId.Value == Thread.CurrentThread.ManagedThreadId, "Method not called from a GUI thread");
		}

		[Conditional("DEBUG")]
		public static void EnsureBgThread()
		{
			if (!allowThreadAsserts) return;
			Debug.Assert(guiThreadId.HasValue, "No Set was called");
			Debug.Assert(guiThreadId.Value != Thread.CurrentThread.ManagedThreadId, "Method not called from a BG thread");
		}

		private class DebugThreadDisable : IDisposable
		{
			public DebugThreadDisable()
			{
				allowThreadAsserts = false;
			}

			public void Dispose()
			{
				allowThreadAsserts = true;
			}
		}

		[Conditional("DEBUG")]
		public static void Break()
		{
			Debugger.Break();
		}
	}
}
