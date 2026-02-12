using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using System.Diagnostics;
using System.IO;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginPowerPoint : PluginOfficeBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public PluginPowerPoint()
		{
			Id = "JobCTRL.PowerPoint";
			ProcessNamesToCheck.Add("powerpnt.exe");
			ChildWindowPredicate = CheckPowerPoint2013OrNewer() ? (Func<ChildWindowInfo, bool>)(w => (w.ClassName == "mdiClass" && w.Caption == string.Empty) || w.ClassName == ProtectedViewClassName) : //PowerPoint 2013
										(w => (w.ClassName == "paneClassDC" && w.Caption == "Slide")); //before PowerPoint 2013
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (ProcessNamesToCheck != null && !ProcessNamesToCheck.Contains(processName))
			{
				return null; //wrong process name
			}

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId, "PowerPoint");

			IDispatch nativeObj = null;
			try
			{
				var childs = EnumChildWindowsHelper.GetChildWindowInfo(hWnd, ChildWindowPredicate);
				var child = GetFirstNonProtectedChild(childs);
				if (child == null)
					return null;
				
				nativeObj = GetNativeObjectFromWindow(child.Handle);
				if (nativeObj == null) return null;
				string documentFullName = ExtractDocumentFullNameFromNativeObject(nativeObj);
				if (documentFullName != null)
				{
					return new Dictionary<string, string>(3)
						{
							{KeyDocumentFullName, documentFullName},
							{KeyDocumentPath, Path.GetDirectoryName(documentFullName)},
							{KeyDocumentFileName, Path.GetFileName(documentFullName)}
						};
				}
			}
			catch (Exception ex)
			{
				log.Debug("Plugin - JobCTRL.PowerPoint: Error while capturing DocumentInfo from PowerPoint.", ex);
				Debug.Print(ex.Message);
			}
			finally
			{
				ComHelper.Release(ref nativeObj);
			}

			return null;
		}

		//JC compiled for .NET 3.5 with X86 won't see registry keys of 64 bit Office
		private bool CheckPowerPoint2013OrNewer()
		{
			try
			{
				for (var ver = 15; ver < 99; ver++)
				{
					var keyFoldName = @"SOFTWARE\Microsoft\Office\" + ver + ".0";
					var offRegFold = Registry.LocalMachine.OpenSubKey(keyFoldName);
					if (offRegFold == null)
					{
						var altKeyFoldName = @"SOFTWARE\Wow6432Node\Microsoft\Office\" + ver + ".0";
						offRegFold = Registry.LocalMachine.OpenSubKey(altKeyFoldName);
						if (offRegFold == null)
							return false;
					}
					var keyName = @"SOFTWARE\Microsoft\Office\" + ver + @".0\PowerPoint\InstallRoot";
					var altKeyName = @"SOFTWARE\Wow6432Node\Microsoft\Office\" + ver + @".0\PowerPoint\InstallRoot";
					if (RegistryHelper.GetValueFromEitherView(RegistryHive.LocalMachine, keyName, altKeyName, "Path") != null) return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				log.Error("CheckPowerPoint2013 failed!", ex);
				return false;
			}
		}

		private string ExtractDocumentFullNameFromNativeObject(IDispatch nativeObj)
		{
			object presentation = null;
			try
			{
				presentation = nativeObj.GetType().InvokeMember("Presentation", BindingFlags.GetProperty, null, nativeObj, null);
				object fullname = presentation.GetType().InvokeMember("FullName", BindingFlags.GetProperty, null, presentation, null);
				return (string)fullname;
			}
			finally
			{
				ComHelper.Release(ref presentation);
			}
		}
	}
}
