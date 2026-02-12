using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderClient
{
	static class InstallNotifyHelper
	{
		private static EventWaitHandle installEventWaitHandle = null;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string EVENT_WAIT_HANDLE_NAME = "JobCTRLInstallEventNameHandle";
		private const string MEMORY_MAPPED_FILE_NAME = "JobCTRLInstallMappedFileName";
		private static string messageBoxText = "Kérem várja meg a telepítés befejezését!";
		private static string messageBoxCaption = "Telepítés folyamatban...";


		public static bool IsInstallerRunning()
		{
			try
			{
				InitializeEventWaitHandle();
				return true;
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				log.Debug("Installation is not running.");
				return false;
			}
		}

		public static void NotifyInstaller()
		{
			MemoryMappedFile mmf = null;
			Stream stream = null;
			try
			{
				messageBoxCaption = Labels.Install_MessageTitle;
				messageBoxText = Labels.Install_MessageText;
				string fullMessage = messageBoxCaption + "\n" + messageBoxText;
				byte[] fullMessageInBytes = Encoding.Unicode.GetBytes(fullMessage);
				var security = new MemoryMappedFileSecurity();
				string user = Environment.UserDomainName + "\\"
						+ Environment.UserName;
				security.AddAccessRule(new AccessRule<MemoryMappedFileRights>(user, MemoryMappedFileRights.FullControl, AccessControlType.Allow));
				mmf = MemoryMappedFile.CreateOrOpen(
					"Local\\" + MEMORY_MAPPED_FILE_NAME,
					fullMessageInBytes.Length,
					MemoryMappedFileAccess.ReadWrite,
					MemoryMappedFileOptions.None,
					security,
					HandleInheritability.Inheritable);
				stream = mmf.CreateViewStream();
				stream.Write(fullMessageInBytes, 0, fullMessageInBytes.Length);
				stream.Flush();
				installEventWaitHandle.Set();
				installEventWaitHandle.WaitOne(2000);
			}
			catch (Exception e)
			{
				log.Error("Something went wrong in notifying the installer.", e);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				if (mmf != null)
					mmf.Dispose();
			}
		}

		private static void InitializeEventWaitHandle()
		{
			try
			{
				installEventWaitHandle = EventWaitHandle.OpenExisting("Local\\" + EVENT_WAIT_HANDLE_NAME, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize);
			}
			catch (WaitHandleCannotBeOpenedException wcboe)
			{
				throw wcboe;
			}
			catch (UnauthorizedAccessException uae)
			{
				log.Warn("Couldn't get installation EventWaitHandler. Trying to change permissions...", uae);
				try
				{
					installEventWaitHandle = EventWaitHandle.OpenExisting("Local\\" + EVENT_WAIT_HANDLE_NAME, EventWaitHandleRights.ReadPermissions | EventWaitHandleRights.ChangePermissions);
					EventWaitHandleSecurity ewhSec = installEventWaitHandle.GetAccessControl();
					string user = Environment.UserDomainName + "\\"
						+ Environment.UserName;
					EventWaitHandleAccessRule rule =
						new EventWaitHandleAccessRule(user,
							EventWaitHandleRights.Modify |
							EventWaitHandleRights.Synchronize,
							AccessControlType.Deny);
					ewhSec.RemoveAccessRule(rule);

					rule = new EventWaitHandleAccessRule(user,
						EventWaitHandleRights.Modify |
						EventWaitHandleRights.Synchronize,
						AccessControlType.Allow);
					ewhSec.AddAccessRule(rule);
					installEventWaitHandle.Close();
					installEventWaitHandle = EventWaitHandle.OpenExisting("Local\\" + EVENT_WAIT_HANDLE_NAME, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize);
				}
				catch (UnauthorizedAccessException e)
				{
					log.Warn("Couldn't change permissions", e);
				}
			}
		}
	}
}
