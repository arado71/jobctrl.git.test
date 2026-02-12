using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public class CaptureExtensionMacAdapter : ICaptureExtensionAdapter
	{
		private static readonly KeyValuePair<CaptureExtensionKey, string>[] emptyCapture = new KeyValuePair<CaptureExtensionKey, string>[0];
		private readonly ICaptureExtension captureExtension;
		private readonly string captureExtensionId;
		private readonly CachedDictionary<IntPtr, List<KeyValuePair<CaptureExtensionKey, string>>> docInfoCache = new CachedDictionary<IntPtr, List<KeyValuePair<CaptureExtensionKey, string>>>(TimeSpan.FromSeconds(3), true);

		public PluginStartInfo CaptureExtensionSettings { get; private set; }

		public CaptureExtensionMacAdapter(Func<PluginStartInfoDetails, ICaptureExtension> extensionFactory, PluginStartInfo settings)
		{
			if (extensionFactory == null) throw new ArgumentNullException("extensionFactory");
			captureExtension = extensionFactory(settings.Details);
			if (captureExtension == null) throw new InvalidOperationException("extensionFactory cannot return null");
			captureExtensionId = captureExtension.Id;
			if (captureExtensionId == null)
			{
				using (captureExtension as IDisposable) { }
				throw new ArgumentException("extension.Id cannot be null");
			}
			CaptureExtensionSettings = settings;
			if (CaptureExtensionSettings == null || CaptureExtensionSettings.Parameters == null) return;
			foreach (var parameter in CaptureExtensionSettings.Parameters)
			{
				captureExtension.SetParameter(parameter.Name, parameter.Value);
			}
		}

		public void SetParameter(string name, string value)
		{
			captureExtension.SetParameter(name, value);
		}

		public Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]> Capture(
			List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow)
		{
			var result = new Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]>();
			if (windowsInfo == null || shouldCaptureWindow == null) return null;

			foreach (var desktopWindow in windowsInfo.ToList()) // copy list to avoid collection enumeration problems
			{
				if (!shouldCaptureWindow(desktopWindow)) continue;
				result.Add(desktopWindow.Handle, Capture(desktopWindow).ToArray());
			}

			return result;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr handle, int pid, string processName)
		{
			return captureExtension.Capture(handle, pid, processName);
		}

		//todo use timer to cancel waiting more than 2 secs
		private IEnumerable<KeyValuePair<CaptureExtensionKey, string>> Capture(DesktopWindow desktopWindow)
		{
			if (desktopWindow == null) return null;
			if (ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableAllPluginCapture)) return emptyCapture;
			List<KeyValuePair<CaptureExtensionKey, string>> retVal;
			var res = Capture(desktopWindow.Handle, desktopWindow.ProcessId, desktopWindow.ProcessName);
			if (res != null)
			{
				retVal = res
							.Where(n => n.Key != null)
							.Select(n => new KeyValuePair<CaptureExtensionKey, string>(new CaptureExtensionKey(captureExtensionId, n.Key), n.Value)).ToList();
				docInfoCache.Set(desktopWindow.Handle, retVal);
			}
			else
			{
				if (!docInfoCache.TryGetValue(desktopWindow.Handle, out retVal))
					return emptyCapture;
			}
			return retVal;
		}

		public void SetCaptureExtensions(List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow)
		{
			if (windowsInfo == null || shouldCaptureWindow == null) return;
			foreach (var desktopWindow in windowsInfo)
			{
				if (!shouldCaptureWindow(desktopWindow)) continue;
				foreach (var capExt in Capture(desktopWindow))
				{
					desktopWindow.SetCaptureExtension(capExt.Key, capExt.Value);
				}
			}
		}

		public void Dispose()
		{
			using (captureExtension as IDisposable) { } //Dispose if disposable
		}
	}
}
