using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using System.Diagnostics;
using System.IO;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginWord : PluginOfficeBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public PluginWord()
		{
			Id = "JobCTRL.Word";
			ProcessNamesToCheck.Add("winword.exe");
			ChildWindowPredicate = w => w.ClassName == "_WwG";
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (ProcessNamesToCheck != null && !ProcessNamesToCheck.Contains(processName))
			{
				return null; //wrong process name
			}

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId, "Word");

			IDispatch nativeObj = null;
			try
			{
				var child = EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, ChildWindowPredicate);
				if (child == null) return null;

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
				log.Debug("Plugin - JobCTRL.Word: Error while capturing DocumentInfo from Word.", ex);
				Debug.Print(ex.Message);
			}
			finally
			{
				ComHelper.Release(ref nativeObj);
			}

			return null;
		}

		private string ExtractDocumentFullNameFromNativeObject(IDispatch nativeObj)
		{
			object document = null;
			try
			{
				document = nativeObj.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, nativeObj, null);
				object fullname = document.GetType().InvokeMember("FullName", BindingFlags.GetProperty, null, document, null);
				return (string)fullname;
			}
			finally
			{
				ComHelper.Release(ref document);
			}
		}
	}
}
