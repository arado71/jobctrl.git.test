using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.InternetExplorer;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//this class can only be used from an STA thread
	public abstract class PluginInternetExplorerBase : PluginDomCaptureBase
	{
		private readonly ILog log;

		public const string KeyStatus = "Status";
		public const string ValueStatusBusy = "Busy";
		private const string ValueStatusNotBusy = "Idle";

		private readonly Dictionary<IntPtr, IEController> controllers = new Dictionary<IntPtr, IEController>();

		protected abstract ChildWindowInfo GetExplorerServerWindow(IntPtr hWnd, int processId, string processName);

		protected PluginInternetExplorerBase(ILog log)
		{
			this.log = log;
		}

		public override IEnumerable<string> GetParameterNames()
		{
			yield return ParamDomCapture;
		}

		public override IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyStatus;
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			DebugEx.EnsureSta();
			PurgeControllersIfApplicable();
			var child = GetExplorerServerWindow(hWnd, processId, processName);
			if (child == null) return null;
			hWnd = child.Handle;

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);

			IEController ctrl;
			if (!controllers.TryGetValue(hWnd, out ctrl) || !ctrl.IsValid)
			{
				try
				{
					ctrl = new IEController(hWnd);
					if (!ctrl.IsValid) return null;
					controllers[hWnd] = ctrl;
				}
				catch (Exception ex)
				{
					log.Debug("Unable to create ie controller", ex);
					return null;
				}
			}
			var result = new Dictionary<string, string>()
			{
				{KeyStatus, ctrl.IsBusy ? ValueStatusBusy : ValueStatusNotBusy},
			};
			foreach (var domCaptureSetting in domCaptureSettings)
			{
				string value;
				if (ctrl.TryGetDomElementProperty(domCaptureSetting, out value))
				{
					result[domCaptureSetting.Key] = value;
				}
			}
			return result;
		}

		private static readonly int purgePeriod = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
		private int lastPurge = Environment.TickCount;
		private void PurgeControllersIfApplicable()
		{
			if ((uint)(Environment.TickCount - lastPurge) < purgePeriod) return;
			lastPurge = Environment.TickCount;
			foreach (var hwnd in controllers
				.Where(n => !n.Value.GetIsValidWithRefresh())
				.Select(n => n.Key)
				.ToArray())
			{
				controllers.Remove(hwnd);
			}
		}
	}
}
