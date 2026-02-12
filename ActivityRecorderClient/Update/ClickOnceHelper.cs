using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using log4net;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;

namespace Tct.ActivityRecorderClient.Update
{
	public static class ClickOnceHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public enum InstallState
		{
			CannotStart,
			InstallClicked,
			StartedButNotClicked
		}

		public static InstallState InstallApplication(string url, bool forceInstall)
		{
			var handlesBefore = new HashSet<IntPtr>(GetClickOnceWindows().Select(n => n.Handle));
			if (!TryStartInstallProcess(url))
			{
				return InstallState.CannotStart;
			}
			if (!forceInstall) return InstallState.StartedButNotClicked;
			int timeout = 120000;//2mins
			var targetTime = Environment.TickCount + timeout;
			while (targetTime - Environment.TickCount > 0)
			{
				foreach (var newWin in GetClickOnceWindows().Where(n => !handlesBefore.Contains(n.Handle)))
				{
					try
					{
						HideWindow(newWin.Handle);

						var root = AutomationElement.FromHandle(newWin.Handle);
						var id = root.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty).ToString();
						if (id == "TrustManagerPromptUI")
						{
							if (TryPressButton(root, "btnInstall"))
							{
								return InstallState.InstallClicked;
							}
						}
					}
					catch
					{
					}
					Thread.Sleep(1);
				}
			}
			foreach (var newWin in GetClickOnceWindows().Where(n => !handlesBefore.Contains(n.Handle)))
			{
				//show possible hidden (error) windows
				ShowWindow(newWin.Handle);
			}
			return InstallState.StartedButNotClicked;
		}

		public static bool StartApplication(string publicKeyToken, string displayName)
		{
			string folder, file, id;
			if (!TryGetShortcutInfo(publicKeyToken, displayName, out folder, out file, out id))
			{
				return false;
			}
			file += ".appref-ms";
			var dirLoc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), folder);
			var fileLoc = Path.Combine(dirLoc, file);
			if (!File.Exists(fileLoc))
			{
				fileLoc = Path.Combine(Path.GetTempPath(), file);
				File.WriteAllBytes(fileLoc, Encoding.Unicode.GetBytes(id)); //we must use Unicode encoding to avoid format exception
			}
			Process.Start(fileLoc);
			return true;
		}

		private static bool TryStartInstallProcess(string url)
		{
			if (string.IsNullOrEmpty(url)) return false;
			if (url.EndsWith(".application"))
			{
				try
				{
					//download file and start it manually so there will be no IE window if there is no internet
					using (var client = new WebClient())
					{
						//Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
						var file = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".application");
						client.DownloadFile(url, file);
						Process.Start(file);
					}
				}
				catch
				{
					return false;
				}
			}
			else
			{
				try
				{
					Process.Start("iexplore.exe", url);
				}
				catch
				{
					return false;
				}
			}
			return true;
		}


		public static bool IsApplicationInstalled()
		{
			string publicKeyToken = GetPublicKeyToken();
			string displayName = ConfigManager.ApplicationName;
			return IsApplicationInstalled(publicKeyToken, displayName);
		}

		public static bool IsApplicationInstalled(string publicKeyToken, string displayName)
		{
			string uninstallString;
			return TryGetUninstallString(publicKeyToken, displayName, out uninstallString);
		}

		public static bool UninstallApplication(bool forceUninstall, bool waitForExit, out bool? forceSuccess, out bool? waitSuccess)
		{
			string publicKeyToken = GetPublicKeyToken();
			string displayName = ConfigManager.ApplicationName;
			return UninstallApplication(publicKeyToken, displayName, forceUninstall, waitForExit, out forceSuccess, out waitSuccess);
		}

		public static string GetPublicKeyToken()
		{
			ApplicationSecurityInfo asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
			byte[] pk = asi.ApplicationId.PublicKeyToken;
			return BitConverter.ToString(pk).Replace("-", "").ToLower();
		}

		public static bool UninstallApplication(string publicKeyToken, string displayName, bool forceUninstall, bool waitForExit, out bool? forceSuccess, out bool? waitSuccess)
		{
			Process uninstallProcess;
			forceSuccess = null;
			waitSuccess = null;
			if (!TryStartUninstallProcess(publicKeyToken, displayName, out uninstallProcess))
			{
				return false;
			}
			if (forceUninstall)
			{
				var win = TryGetMaintenanceForm(displayName, 60000); //1min
				log.Debug("Maintenance form has " + (win != null ? "": "not ") + "been found.");
				//there might be some maintenance issue after starting uninstall string already (no maintenance form displayed)
				if (win != null) 
				{
					//ShowWindowAsync(win.Handle, WindowShowStyle.ShowMinimized); //Automation does not work on hidden or minimized windows
					//user won't see this window (might see some flickering)
					//new desktop might be a better solution here but this is ok for now
					HideWindow(win.Handle);

#if !NET4
					Thread.Sleep(2000); //Try to avoid process exit caused by AccessViolationException occuring in UIAutomation.
#endif
					forceSuccess = TryClickOkOnMaintenanceForm(win, 60000);	//1min
				}
			}

			if (waitForExit)
			{
				log.Debug("Waiting for uninstall process to exit.");
				waitSuccess = uninstallProcess.WaitForExit(300000); //5mins
				log.Debug("Uninstall process' WaitForExit returned: " + waitSuccess);
				if (!waitSuccess.Value) //there might be some maintenance issue
				{
					foreach (var dfsvcWin in GetClickOnceWindows())
					{
						//bring dialog infront of the user
						ShowWindow(dfsvcWin.Handle);
					}
				}
			}
			var success = (!forceSuccess.HasValue || forceSuccess.Value) && (!waitSuccess.HasValue || waitSuccess.Value);
			if (success && forceUninstall && waitForExit)
			{
				//double check for success
				return !IsApplicationInstalled(publicKeyToken, displayName);
			}
			return success;
		}

		private static bool TryStartUninstallProcess(string publicKeyToken, string displayName, out Process uninstallProcess)
		{
			try
			{
				string uninstallString;
				if (!TryGetUninstallString(publicKeyToken, displayName, out uninstallString))
				{
					uninstallProcess = null;
					return false;
				}

				//uninstallString looks like this:
				//rundll32.exe dfshim.dll,ShArpMaintain JobCTRL.application, Culture=neutral, PublicKeyToken=c7a30084241735b2, processorArchitecture=msil
				//but this won't work for this:
				//c:\Program Files\Microsoft Silverlight\4.0.50524.0\Silverlight.Configuration.exe -uninstallApp 3722037388.www.followcup.com
				var prgAndArgs = uninstallString.Split(new[] { ' ' }, 2);
				uninstallProcess = Process.Start(prgAndArgs[0], prgAndArgs[1]);
				return uninstallProcess != null;
			}
			catch (Exception ex)
			{
				log.Error("Unable to start uninstall process.", ex);
				uninstallProcess = null;
				return false;
			}
		}

		private static IEnumerable<WindowInfo> GetClickOnceWindows()
		{
			return EnumWindowsHelper.GetWindowsInfo()
				.Where(n => n.ProcessName == "dfsvc.exe")
				.Where(n => n.ClassName != "tooltips_class32");
		}

		private static WindowInfo TryGetMaintenanceForm(string displayName, int timeout)
		{
			try
			{
				WindowInfo win;
				int targetTime = Environment.TickCount + timeout;
				while ((win = GetClickOnceWindows()
					.Where(n => n.Title.StartsWith(displayName)) //'Maintenance' part might be localized
					.FirstOrDefault()) == null)
				{
					if (targetTime - Environment.TickCount < 0) return null;
					Thread.Sleep(1);
				}
				return win;
			}
			catch (Exception ex)
			{
				log.Error("Unable to get maintenance form.", ex);
				return null;
			}
		}

		private static bool TryClickOkOnMaintenanceForm(WindowInfo win, int timeout)
		{
			var success = TryClickOkOnMaintenanceForm(win);
			var targetTime = Environment.TickCount + timeout;
			while (!success)
			{
				if (targetTime - Environment.TickCount < 0) return false;
				Thread.Sleep(100);
				success = TryClickOkOnMaintenanceForm(win);
			}
			return true;
		}

#if NET4
		[System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
		[System.Security.SecurityCritical]
#endif
		private static bool TryClickOkOnMaintenanceForm(WindowInfo win)
		{
			AutomationElement root = null;
			bool? foundRadio = null, foundButton = null;
			try
			{
				log.Debug("Retrieving automation root element.");
				root = AutomationElement.FromHandle(win.Handle);
				if (root == null) return false;

				//select remove application radio buttom
				log.Debug("Try to select radio button.");
				foundRadio = TrySelectRadio(root, "radioRemove");
				if (!foundRadio.Value)
				{
					return false;
				}
				//press OK button
				log.Debug("Try to press OK button.");
				foundButton = TryPressButton(root, "btnOk");
				if (!foundButton.Value)
				{
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Failed to clik ok on maintenance form.", ex);
				return false;
			}
			finally
			{
				log.Debug("TryClickOkOnMaintenanceForm Root is " + (root == null ? "" : "not ") + "null foundRadio: " + foundRadio + " foundButton: " + foundButton);
			}
		}

		private static bool TryGetShortcutInfo(string publicKeyToken, string displayName, out string folderName, out string fileName, out string appId)
		{
			//set up the string to search for
			string searchString = "PublicKeyToken=" + publicKeyToken;
			try
			{
				//open the registry key and get the subkey names 
				using (RegistryKey uninstallKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall"))
				{
					string[] appKeyNames = uninstallKey.GetSubKeyNames();

					//search through the list for one with a match 
					foreach (string appKeyName in appKeyNames)
					{
						using (RegistryKey appKey = uninstallKey.OpenSubKey(appKeyName))
						{
							string id = (string)appKey.GetValue("ShortcutAppId");
							string name = (string)appKey.GetValue("DisplayName");
							if (name == displayName && id != null && id.Contains(searchString))
							{
								appId = id;
								folderName = (string)appKey.GetValue("ShortcutFolderName");
								fileName = (string)appKey.GetValue("ShortcutFileName");
								return true;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to get shortcut info.", ex);
			}
			folderName = null;
			fileName = null;
			appId = null;
			return false;
		}

		private static bool TryGetUninstallString(string publicKeyToken, string displayName, out string uninstallString)
		{
			//set up the string to search for
			string searchString = "PublicKeyToken=" + publicKeyToken;

			try
			{
				//open the registry key and get the subkey names 
				using (RegistryKey uninstallKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall"))
				{
					string[] appKeyNames = uninstallKey.GetSubKeyNames();

					//search through the list for one with a match 
					foreach (string appKeyName in appKeyNames)
					{
						using (RegistryKey appKey = uninstallKey.OpenSubKey(appKeyName))
						{
							string uninsStr = (string)appKey.GetValue("UninstallString");
							string name = (string)appKey.GetValue("DisplayName");
							if (name == displayName && uninsStr != null && uninsStr.Contains(searchString))
							{
								uninstallString = uninsStr;
								return true;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to get uninstall string info.", ex);
			}
			displayName = null;
			uninstallString = null;
			return false;
		}

		private static void ShowWindow(IntPtr hWnd)
		{
			SetWindowPos(hWnd, SpecialWindowHandles.HWND_TOPMOST, 10, 10, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
		}

		private static void HideWindow(IntPtr hWnd)
		{
			//todo don't show in taskbar ? http://social.msdn.microsoft.com/Forums/en-US/vbgeneral/thread/64bb2f43-9104-4e92-aa0f-5d9ae283e2e6/
			SetWindowPos(hWnd, SpecialWindowHandles.HWND_TOPMOST, -4000, -4000, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
		}

		private static bool TryPressButton(AutomationElement root, string automationId)
		{
			try
			{
				Condition idCond = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
				Condition typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button);
				var walker = new TreeWalker(new AndCondition(typeCond, idCond));
				var button = walker.GetFirstChild(root);
				if (button == null) return false;
				var invokePattern = (InvokePattern)button.GetCurrentPattern(InvokePattern.Pattern);
				invokePattern.Invoke();
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Unable to press button.", ex);
				return false;
			}
		}

		private static bool TrySelectRadio(AutomationElement root, string automationId)
		{
			try
			{
				Condition idCond = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
				Condition typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton);
				var walker = new TreeWalker(new AndCondition(typeCond, idCond));
				var radioToSelect = walker.GetFirstChild(root);
				if (radioToSelect == null) return false;
				var selectPattern = (SelectionItemPattern)radioToSelect.GetCurrentPattern(SelectionItemPattern.Pattern);
				selectPattern.Select();
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Unable to select radio button.", ex);
				return false;
			}
		}

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		private static extern bool SetWindowPos(
			 IntPtr hWnd,                          // window handle
			 SpecialWindowHandles hWndInsertAfter, // placement-order handle
			 int X,                                // horizontal position
			 int Y,                                // vertical position
			 int cx,                               // width
			 int cy,                               // height
			 SetWindowPosFlags uFlags);            // window positioning flags

		/// <summary>
		///     Special window handles
		/// </summary>
		public enum SpecialWindowHandles
		{
			// ReSharper disable InconsistentNaming
			/// <summary>
			///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
			/// </summary>
			HWND_TOP = 0,
			/// <summary>
			///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
			/// </summary>
			HWND_BOTTOM = 1,
			/// <summary>
			///     Places the window at the top of the Z order.
			/// </summary>
			HWND_TOPMOST = -1,
			/// <summary>
			///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
			/// </summary>
			HWND_NOTOPMOST = -2
			// ReSharper restore InconsistentNaming
		}


		[Flags]
		public enum SetWindowPosFlags : uint
		{
			// ReSharper disable InconsistentNaming

			/// <summary>
			///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
			/// </summary>
			SWP_ASYNCWINDOWPOS = 0x4000,

			/// <summary>
			///     Prevents generation of the WM_SYNCPAINT message.
			/// </summary>
			SWP_DEFERERASE = 0x2000,

			/// <summary>
			///     Draws a frame (defined in the window's class description) around the window.
			/// </summary>
			SWP_DRAWFRAME = 0x0020,

			/// <summary>
			///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
			/// </summary>
			SWP_FRAMECHANGED = 0x0020,

			/// <summary>
			///     Hides the window.
			/// </summary>
			SWP_HIDEWINDOW = 0x0080,

			/// <summary>
			///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
			/// </summary>
			SWP_NOACTIVATE = 0x0010,

			/// <summary>
			///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
			/// </summary>
			SWP_NOCOPYBITS = 0x0100,

			/// <summary>
			///     Retains the current position (ignores X and Y parameters).
			/// </summary>
			SWP_NOMOVE = 0x0002,

			/// <summary>
			///     Does not change the owner window's position in the Z order.
			/// </summary>
			SWP_NOOWNERZORDER = 0x0200,

			/// <summary>
			///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
			/// </summary>
			SWP_NOREDRAW = 0x0008,

			/// <summary>
			///     Same as the SWP_NOOWNERZORDER flag.
			/// </summary>
			SWP_NOREPOSITION = 0x0200,

			/// <summary>
			///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
			/// </summary>
			SWP_NOSENDCHANGING = 0x0400,

			/// <summary>
			///     Retains the current size (ignores the cx and cy parameters).
			/// </summary>
			SWP_NOSIZE = 0x0001,

			/// <summary>
			///     Retains the current Z order (ignores the hWndInsertAfter parameter).
			/// </summary>
			SWP_NOZORDER = 0x0004,

			/// <summary>
			///     Displays the window.
			/// </summary>
			SWP_SHOWWINDOW = 0x0040,

			// ReSharper restore InconsistentNaming
		}
		
		private class WindowInfo
		{
			public IntPtr Handle { get; set; }
			public string Title { get; set; }
			public string ProcessName { get; set; }
			public string ClassName { get; set; }

			public override string ToString()
			{
				return ProcessName + " @ " + Title;
			}
		}

		private static class EnumWindowsHelper
		{
			private class WindowInfoBuilder
			{
				public readonly List<WindowInfo> WindowsInfo = new List<WindowInfo>();

				public bool ProcessWindow(IntPtr hWnd, int lParam)
				{
					var window = new WindowInfo() { Handle = hWnd };

					if (!IsWindowVisible(hWnd)) return true;
					window.Title = GetWindowText(hWnd);
					window.ProcessName = GetWindowThreadProcessNameFromCache(hWnd);
					window.ClassName = GetClassName(hWnd);

					WindowsInfo.Add(window);

					return true; //process next window
				}

				private readonly Dictionary<int, string> processNameCache = new Dictionary<int, string>();
				private string GetWindowThreadProcessNameFromCache(IntPtr hWnd)
				{
					int procId;
					GetWindowThreadProcessId(hWnd, out procId);
					string result;
					if (!processNameCache.TryGetValue(procId, out result))
					{
						if (ProcessNameHelper.TryGetProcessName(procId, hWnd, out result))
						{
							processNameCache.Add(procId, result);
						}
					}
					return result;
				}
			}

			public static List<WindowInfo> GetWindowsInfo()
			{
				var builder = new WindowInfoBuilder();
				EnumWindows(builder.ProcessWindow, 0);
				return builder.WindowsInfo;
			}

			private static string GetClassName(IntPtr hWnd)
			{
				int length = 64;
				while (true)
				{
					StringBuilder sb = new StringBuilder(length);
					GetClassName(hWnd, sb, sb.Capacity);
					if (sb.Length != length - 1)
					{
						return sb.ToString();
					}
					length *= 2;
				}
			}

			//private static string GetWindowThreadProcessName(IntPtr hWnd)
			//{
			//    int procId;
			//    GetWindowThreadProcessId(hWnd, out procId);
			//    //return procId.ToString();
			//    return Process.GetProcessById(procId).ProcessName;
			//    //return Process.GetProcessById(procId).MainModule.ModuleName;
			//}

			private static string GetWindowText(IntPtr hWnd)
			{
				int windowTextLength = GetWindowTextLength(hWnd) + 1;
				StringBuilder activeWindowText = new StringBuilder(windowTextLength);

				GetWindowText(hWnd, activeWindowText, windowTextLength);
				return activeWindowText.ToString();
			}

			private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

			[DllImport("user32.dll", EntryPoint = "EnumWindows")]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, int lParam);

			[DllImport("user32.dll", EntryPoint = "GetClassName")]
			private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

			[DllImport("user32.dll", EntryPoint = "GetWindowText")]
			private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

			[DllImport("user32.dll", EntryPoint = "GetWindowTextLength")]
			private static extern int GetWindowTextLength(IntPtr hWnd);

			[DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
			private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);
		}

		[DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowVisible(IntPtr hWnd);
	}
}
