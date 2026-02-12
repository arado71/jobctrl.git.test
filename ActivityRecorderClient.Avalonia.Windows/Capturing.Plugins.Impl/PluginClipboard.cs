using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//todo use event driven change notifications
	//todo SourceXY info is lost when creating a new plugin, so we should implement plugin reuse if no params have changed or use monostate pattern?
	//use from BG thread: http://stackoverflow.com/questions/5944605/c-sharp-clipboard-gettext
	//use notifications:  http://www.pinvoke.net/default.aspx/user32.setclipboardviewer
	//use notifications:  http://www.developer.com/net/csharp/article.php/3359891/C-Tip-Monitoring-Clipboard-Activity-in-C.htm
	//use notifications:  http://www.radsoftware.com.au/articles/clipboardmonitor.aspx
	//general info:       http://msdn.microsoft.com/en-us/library/windows/desktop/ms649016%28v=vs.85%29.aspx
	//http://code.google.com/p/pinvoke/source/browse/trunk/System.PInvoke/Windows/Clipboard.cs
	/// <summary>
	/// Plugin for getting Clipboard text.
	/// </summary>
	public class PluginClipboard : ICaptureExtension, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public const string PluginId = "JobCTRL.Clipboard";
		private const string ParamMaxLength = "MaxLength";
		private const string KeyText = "Text";
		private const string KeySourceProcessName = "SourceProcessName";
		private const string KeySourceTitle = "SourceTitle";
		private const string KeySourceUrl = "SourceUrl";

		private readonly ManualResetEvent mreStopped = new ManualResetEvent(false);
		private readonly Thread thread;
		private readonly object thisLock = new object(); //protects the following four fields
		private string clipText = "";
		private string clipProcessName = "Idle";
		private string clipTitle = "";
		private string clipUrl = null;
		private volatile int maxLength = 4000;
		private uint seq;
		private bool firstRun = true;

		public PluginClipboard()
		{
			thread = new Thread(ThreadLoop);
			thread.SetApartmentState(ApartmentState.STA);
			thread.Name = "PC";
			thread.IsBackground = true;
			thread.Start();
		}

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamMaxLength;
		}

		public void SetParameter(string name, string value)
		{
			if (!string.Equals(name, ParamMaxLength, StringComparison.OrdinalIgnoreCase)) return;
			int val;
			if (int.TryParse(value, out val))
			{
				maxLength = val;
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyText;
			yield return KeySourceProcessName;
			yield return KeySourceTitle;
			yield return KeySourceUrl;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			lock (thisLock)
			{
				return new[] { 
					new KeyValuePair<string, string>(KeyText, clipText),
					new KeyValuePair<string, string>(KeySourceProcessName, clipProcessName),
					new KeyValuePair<string, string>(KeySourceTitle, clipTitle),
					new KeyValuePair<string, string>(KeySourceUrl, clipUrl),
				};
			}
		}

		public void ThreadLoop(object state)
		{
			do
			{
				var curr = WinApi.GetClipboardSequenceNumber(); //curr == 0 means we cannot get seq number
				if (seq == curr && curr != 0) continue;
				try
				{
					if (curr == 0 && seq != 0) log.Error("Unable to get clipboard sequence number");
					//we could use DesktopCaptureService but probably that's too heavyweight so we use copy paste code reuse instead
					var activeHWnd = firstRun ? IntPtr.Zero : WinApi.GetForegroundWindow(); //on the first run (when plugin starts) GetForegroundWindow would be a lie... (use Idle instead)
					firstRun = false;
					int processId;
					WinApi.GetWindowThreadProcessId(activeHWnd, out processId);
					var title = WindowTextHelper.GetWindowText(activeHWnd);
					var processName = DesktopCaptureWinService.ResolveProcessNameFromId(processId, activeHWnd); //no need to cache as clipboard won't change frequently
					var url = DesktopCaptureWinService.GetUrlFromBrowser(activeHWnd, processName); //todo what if this fails? (we should retry a few times in the next few loops)

					var text = Clipboard.GetText(); //it would be nice if we could limit the length here.
					if (text != null && text.Length > maxLength)
					{
						text = text.Substring(0, maxLength);
						log.Debug("Clipboard data is truncated");
					}
					lock (thisLock)
					{
						clipText = text;
						clipProcessName = processName;
						clipTitle = title;
						clipUrl = url;
					}
					seq = curr;
					var textToLog = log.Logger.IsEnabledFor(log4net.Core.Level.Verbose) ? text : "*C*";
					log.DebugFormat("Clipboard changed ({0}) data: {1} source: {2} {3} {4}", curr, textToLog, processName, title, url);
				}
				catch (Exception ex)
				{
					log.Error("Unable to get clipboard data", ex);
				}
			} while (!mreStopped.WaitOne(200));
		}

		private int isDisposed;
		public void Dispose()
		{
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			mreStopped.Set();
			thread.Join();
			mreStopped.Close();
		}
	}
}
