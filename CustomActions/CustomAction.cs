using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using WixToolset.Dtf.WindowsInstaller;

namespace CustomActions
{
	public class CustomActions
	{
		private static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
		{
			try
			{
				using (File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
				{
				}
				return true;
			}
			catch
			{
				if (throwIfFails) throw;
				return false;
			}
		}

		private static string actionName;

		private static void Log(Session session, string text)
		{
			session.Log("{0} ({1}): {2}", DateTime.Now, actionName, text);
		}

		private static void Log(Session session, string format, params object[] pars)
		{
			Log(session, string.Format(format, pars));
		}

		[CustomAction]
		public static ActionResult StopApp(Session session)
		{
			actionName = "StopApp";
			try
			{
				Log(session, "Begin");
				var appRunPath = session["AppRunPath"];
				var uiLevel = session["UILevel"];

				var appRootDir = Path.GetDirectoryName(appRunPath);
				var outlookSyncDir = Path.Combine(appRootDir, "OutlookSync");
				var lotusSyncDir = Path.Combine(appRootDir, "LotusNotesSync");
				var chromeInteropOldDir = Path.Combine(appRootDir, "ChromeInterop");
				var jcSubprocessPaths = new[]
				{
					Path.Combine(outlookSyncDir, "JC.Meeting.exe"),
					Path.Combine(outlookSyncDir, "JC.Meeting64.exe"),
					Path.Combine(lotusSyncDir, "JC.Meeting.exe"),
					Path.Combine(lotusSyncDir, "JC.Meeting64.exe"),
					Path.Combine(outlookSyncDir, "JC.Mail.exe"),
					Path.Combine(outlookSyncDir, "JC.Mail64.exe"),
					Path.Combine(chromeInteropOldDir, "JC.Chrome.exe")
				};

				var processes = Process.GetProcesses()
					.Where(p => string.Equals(p.GetFileNameSafe(), appRunPath, StringComparison.CurrentCultureIgnoreCase) && !p.HasExited).ToList();
				if (processes.Any() &&
					("2".Equals(uiLevel) ||
					 session.Message(
						 InstallMessage.Error | (InstallMessage)MessageIcon.Warning |
						 (InstallMessage)MessageButtons.YesNo, new Record
						 {
							 FormatString = "There is a running JobCTRL process and it will be stopped."
						 }) == MessageResult.Yes))
				{
					try
					{
						Log(session, "Trying to stop service if running");
						var sc = new ServiceController("JobCTRL:JcAppChkService");
						sc.Stop();
						sc.WaitForStatus(ServiceControllerStatus.Stopped);
					}
					catch (InvalidOperationException)
					{
						Log(session, "No service running, do nothing");
					}

					foreach (var process in processes.Where(process => !process.HasExited))
					{
						var sessionId = process.SessionId;
						var path = process.MainModule?.FileName;
						try
						{
							Log(session, "Killing PID:{0}, Name:{1}", process.Id, process.ProcessName);
							process.Kill();
						}
						catch (InvalidOperationException)
						{
							// do nothing, service already exited
						}

						try
						{
							if (!"2".Equals(uiLevel)) continue;
							var taskService = new TaskService();
							var task = taskService.NewTask();
							var action = new ExecAction(path, "SuppressRunningNotification");
							var userId = GetProcessOwner(sessionId);
							task.Principal.UserId = userId;
							task.Actions.Add(action);
							var time = DateTime.Now.AddMinutes(2);
							task.Triggers.Add(new TimeTrigger(time) { EndBoundary = time.AddMinutes(1) });
							task.Settings.DisallowStartIfOnBatteries = false;
							task.Settings.StopIfGoingOnBatteries = false; 
							task.Settings.DeleteExpiredTaskAfter = TimeSpan.FromDays(1);
							task.Settings.ExecutionTimeLimit = TimeSpan.Zero;
							var name = "JcStartAfterUpdate" + userId.GetHashCode().ToString("X8");
							taskService.RootFolder.DeleteTask(name, false);
							taskService.RootFolder.RegisterTaskDefinition(name, task);
							Log(session, $"Created task name: {name} path: {path} userId: {userId} at: {time:G}");
						}
						catch (Exception ex)
						{
							Log(session, "Scheduling execute failed with message: " + ex);
						}
					}
				}
				var are1 = new AutoResetEvent(false);
				var active = 0;
				var subProcesses = Process.GetProcesses().Where(p =>
				{
					var name = p.GetFileNameSafe();
					return jcSubprocessPaths.Any(s => name.Equals(s, StringComparison.InvariantCultureIgnoreCase)) && !p.HasExited;
				}).ToList();
				Log(session, "Checking subprocesses");
				foreach (var process in subProcesses)
				{
					process.EnableRaisingEvents = true;
					var pid = process.Id;
					var name = process.ProcessName;
					Log(session, "Checking PID:{0}, Name:{1}", pid, name);
					Interlocked.Increment(ref active);
					process.Exited += (sender, args) => { Interlocked.Decrement(ref active); are1.Set(); Log(session, "PID:{0}, Name:{1} exited", pid, name); };
				}
				while (active > 0)
				{
					var now = active;
					Log(session, "Waiting for {0} process(es) to exit", now);
					are1.WaitOne(60000);
					if (now == active) break;
				}
				foreach (var process in subProcesses.Where(p => !p.HasExited))
				{
					try
					{
						Log(session, "Killing PID:{0}, Name:{1}", process.Id, process.ProcessName);
						process.Kill();
					}
					catch (InvalidOperationException)
					{
						// do nothing, service already exited
					}
				}

				Log(session, "End");
				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(session, "StopApp failed with message: " + ex.Message);
				return ActionResult.Success;
			}
		}

		private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
		private const int WTS_UserName = 5;

		private static string GetProcessOwner(int sessionId)
		{
			if (WTSQuerySessionInformationW(WTS_CURRENT_SERVER_HANDLE,
				sessionId,
				WTS_UserName,
				out var answerBytes,
				out _))
			{
				return Marshal.PtrToStringUni(answerBytes);
			}

			return "NO OWNER";
		}
		[DllImport("Wtsapi32.dll")]
		private static extern bool WTSQuerySessionInformationW(
			IntPtr hServer,
			int SessionId,
			int WTSInfoClass,
			out IntPtr ppBuffer,
			out IntPtr pBytesReturned);

		[CustomAction]
		public static ActionResult CheckShortcut(Session session)
		{
			session.Log("Begin CheckShortcut");
			session.Log("Checking directory: " + session["CHECKDIR"]);
			if (!IsDirectoryWritable(session["CHECKDIR"]))
			{
				session.Log("Unable to create file in directory, setting SUPPRESSSHORTCUT property");
				session["SUPRESSSHORTCUT"] = "1";
			}
			else
			{
				session.Log("File creation was successful");
			}

			session.Log("End CheckShortcut");
			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult UnregisterAddons(Session session)
		{
			actionName = "UnregisterAddons";
			try
			{
				Log(session, "Begin");
				var appRunPath = session["AppRunPath"];

				var appRootDir = Path.GetDirectoryName(appRunPath);
				var outlookAddinLocHash = CreateConsistentHashCode(appRootDir).ToString("X");
				var addinName = string.Format("MailActivityTracker{0}", outlookAddinLocHash);
				var addinKey = @"Software\Microsoft\Office\Outlook\Addins\" + addinName;
				Log(session, "Removing {0} registry key", addinKey);
				Registry.CurrentUser.DeleteSubKeyTree(addinKey);
				Log(session, "End");
				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(session, "failed with message: " + ex.Message);
				return ActionResult.Success;
			}
		}

		[CustomAction]
		public static ActionResult DeleteBrowserExtensions(Session session)
		{
			actionName = "DeleteBrowserExtensions";
			Log(session, "Begin");

			try
			{
				var regKey = @"SOFTWARE\Google\Chrome\Extensions\obmlbfkihnobgbokahopnbaehffncfoe";
				DeleteFromRegistry(session, regKey, null, true);

				regKey = @"Software\Google\Chrome\NativeMessagingHosts\com.tct.jobctrl";
				DeleteFromRegistry(session, regKey, null, true);

				regKey = @"SOFTWARE\Policies\Google\Chrome\ExtensionInstallForcelist";
				var regName = "7500";
				if (IsHKLMRegistryItemExisting(regKey, regName))
				{
					Log(session, "Chrome HKLM item found");
					DeleteFromRegistry(session, regKey, regName, false);
				}

				regKey = @"SOFTWARE\Microsoft\Edge\Extensions\obmlbfkihnobgbokahopnbaehffncfoe";
				DeleteFromRegistry(session, regKey, null, true);

				regKey = @"Software\Microsoft\Edge\NativeMessagingHosts\com.tct.jobctrl";
				DeleteFromRegistry(session, regKey, null, true);

				regKey = @"SOFTWARE\Policies\Microsoft\Edge\ExtensionInstallForcelist";
				regName = "7500";
				if (IsHKLMRegistryItemExisting(regKey, regName))
				{
					Log(session, "Edge HKLM item found");
					DeleteFromRegistry(session, regKey, regName, false);
				}

				var edgeId = "com.jobctrl.jobctrl_0.0.1.6_neutral__exy19pxymbyha";
				using (var PowerShellInstance = System.Management.Automation.PowerShell.Create())
				{
					PowerShellInstance.AddScript("Remove-AppxPackage -Package " + edgeId);
					PowerShellInstance.Invoke();
				}
			}
			catch (Exception ex)
			{
				Log(session, "End with exception: " + ex.Message);
				return ActionResult.Failure;
			}

			Log(session, "End");
			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult RemoveJcAutorunKeyFromRegistry(Session session)
		{
			actionName = "RemoveJcAutorunKeyFromRegistry";
			return RemoveAutorunKeyFromRegistry(session, "JobCTRL");
		}

		[CustomAction]
		public static ActionResult RemoveJcAutorunKeyFromMachineRegistry(Session session)
		{
			actionName = "RemoveJcAutorunKeyFromRegistry";
			return RemoveAutorunKeyFromRegistry(session, "JobCTRL", true);
		}

		[CustomAction]
		public static ActionResult RemoveVoxAutorunKeyFromRegistry(Session session)
		{
			actionName = "RemoveVoxAutorunKeyFromRegistry";
			return RemoveAutorunKeyFromRegistry(session, "VoxCTRL");
		}

		[CustomAction]
		public static ActionResult CheckPendingRestart(Session session)
		{
			actionName = "CheckPendingRestart";
			try
			{
				Log(session, "Begin");
				var subkey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.QueryValues);
				var value = subkey?.GetValue("PendingFileRenameOperations");
				if (value != null)
				{
					var uiLevel = session["UILevel"];
					Log(session, "UILevel: " + uiLevel);
					Log(session, "PendingFileRenameOperations entry exists");
					if (!uiLevel.Equals("2"))
						session.Message(
						    InstallMessage.User | (InstallMessage)MessageIcon.Warning |
						    (InstallMessage)MessageButtons.OK, new Record
						    {
							    FormatString = "There is a restart pending. Please restart your computer before  attempting to install this application."
							}) ;
					return ActionResult.Failure;
				}
				Log(session, "End");
				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Log(session, "failed with message: " + ex.Message);
				return ActionResult.Success;
			}
		}

		private static ActionResult RemoveAutorunKeyFromRegistry(Session session, string appName, bool isMachineLevel = false)
		{
			Log(session, "Begin");

			try
			{
				var subkey = (isMachineLevel ? Registry.LocalMachine : Registry.CurrentUser).OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.SetValue);
				subkey?.DeleteValue(appName, false);
			}
			catch (Exception ex)
			{
				Log(session, "End with exception: " + ex.Message);
				return ActionResult.Failure;
			}

			Log(session, "End");
			return ActionResult.Success;
		}

		private static bool IsHKLMRegistryItemExisting(string key, string name)
		{
			var subKey = Registry.LocalMachine.OpenSubKey(key);
			if (subKey != null && subKey.GetValue(name) != null)
			{
				return true;
			}
			return false;
		}

		private static void DeleteFromRegistry(Session session, string key, string name, bool isHKCU)
		{
			try
			{
				if (isHKCU)
				{
					if (Registry.CurrentUser.OpenSubKey(key) != null)
					{
						Registry.CurrentUser.DeleteSubKey(key);
						Log(session, key + " is deleted");
					}
					else
					{
						Log(session, key + " is not found");
					}
				}
				else
				{
					if (name == null)
					{
						Log(session, key + ": value can't be null");
						return;
					}
					var subKey = Registry.LocalMachine.OpenSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.SetValue);
					if (subKey != null)
					{
						subKey.DeleteValue(name, true);
						Log(session, key + "/" + name + " is deleted");
					}
					else
					{
						Log(session, key + " is not found");
					}
				}
			}
			catch (SecurityException ex)
			{
				Log(session, key + " delete failed with message: " + ex.Message + "SecurityException");
				var uiLevel = session["UILevel"];
				Log(session, "UILevel: " + uiLevel);
				if ((!uiLevel.Equals("2")) && session.Message(
							 InstallMessage.User | (InstallMessage)MessageIcon.Warning |
							 (InstallMessage)MessageButtons.YesNo, new Record
							 {
								FormatString = "The msi can't delete the browser extensions (Chrome, Edge) without admin rights. " +
												"If you can grant them you should abort this uninstall and rerun it as an admin. " +
												"If you can't, then the progress can be continued. Do you abort the uninstall?"
							 }) == MessageResult.Yes)
				{
					Log(session, "Answer: Abort");
					throw new Exception("Abort uninstall");
				}
				Log(session, key + "/" + name + " can't be deleted");
			}
			catch (Exception ex)
			{
				Log(session, key + " delete failed with message: " + ex.Message);
			}
		}

		[CustomAction]
		public static ActionResult DeletePersonalData(Session session)
		{
			actionName = "DeletePersonalData";
			Log(session, "Begin");

			var appRunPath = session["AppRunPath"];
			Log(session, "JobCTRL: " + appRunPath);

			var uiLevel = session["UILevel"];
			Log(session, "UILevel: " + uiLevel);
			if ((!uiLevel.Equals("2")) && session.Message(
						 InstallMessage.User | (InstallMessage)MessageIcon.Warning |
						 (InstallMessage)MessageButtons.YesNo, new Record
						 {
							 FormatString = "Do you want to remove all the personal data including logs?"
						 }) == MessageResult.Yes)
			{
				KillJcChrome(session);

				string cmd = "\"" + appRunPath + "\"" + " DeletePersonalData";
				var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + cmd + "\"")
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true
				};
				var process = Process.Start(processInfo);
				process.WaitForExit();
				string output = process.StandardOutput.ReadLine();
				Log(session, "output: " + output);
				process.Close();

				if (!output.EndsWith("0"))
				{
					Log(session, "Some files can't be deleted");
				}
			}

			Log(session, "End");
			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult SetLoginData(Session session)
		{
			actionName = "SetLoginData";
			Log(session, "Begin");

			var appRunPath = session["AppRunPath"];
			Log(session, "JobCTRL: " + appRunPath);
			string username = session["LOGINUSERNAME"];
			string password = session["LOGINPASSWORD"];
			if (!string.IsNullOrEmpty(username))
			{
				if(string.IsNullOrEmpty(password))
				{
					Log(session, "Username set but missing password, logindata will not be set.");
					Log(session, "End");
					return ActionResult.Success;
				}
				Log(session, "Setting username (" + username + ") with password (********) as login data.");
				string cmd = "\"" + appRunPath + "\"" + " SetLoginData \"" + username + "\" \"" + password + "\"";
				var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + cmd + "\"")
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true
				};
				var process = Process.Start(processInfo);
				process.WaitForExit();
				string output = process.StandardOutput.ReadLine();
				Log(session, "output: " + output);
				process.Close();

				if (!output.EndsWith("0"))
				{
					Log(session, "There was a problem setting the login data.");
				}
			}

			Log(session, "End");
			return ActionResult.Success;
		}

		private static void KillJcChrome(Session session)
		{
			var processes = Process.GetProcessesByName("JC.Chrome");
			if (processes.Length == 1)
			{
				processes[0].Kill();
				Log(session, "JC.Chrome killed");
			}
		}

		/// <summary>
		/// Consistent hash value for strings on both x86 and x64 platforms
		/// based on http://stackoverflow.com/questions/8838053/can-object-gethashcode-produce-different-results-for-the-same-objects-strings
		/// </summary>
		private static int CreateConsistentHashCode(string obj)
		{
			if (obj == null)
				return 0;
			int hash = obj.Length;
			for (int i = 0; i != obj.Length; ++i)
				hash = (hash << 5) - hash + obj[i];
			return hash;
		}
	}

	static class Extensions
	{
		internal static string GetFileNameSafe(this Process process)
		{
			try
			{
				return process.MainModule.FileName;
			}
			catch (Win32Exception)
			{
				return "*";
			}
		}

	}
}
