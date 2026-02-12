using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using System.Diagnostics;
using System.IO;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginExcel : PluginOfficeBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly CultureInfo ci = new CultureInfo("en-US");

		public PluginExcel()
		{
			Id = "JobCTRL.Excel";
			ProcessNamesToCheck.Add("excel.exe");
			ChildWindowPredicate = w => w.ClassName == "EXCEL7" || w.ClassName == ProtectedViewClassName;
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (ProcessNamesToCheck != null
				&& !ProcessNamesToCheck.Contains(processName))
			{
				return null; //wrong process name
			}

			AppVersionLogger.LogAssemblyVersionFromProcId((uint) processId, "Excel");

			IDispatch nativeObj = null;
			try
			{
				var childs = EnumChildWindowsHelper.GetChildWindowInfo(hWnd, ChildWindowPredicate);
				var child = GetFirstNonProtectedChild(childs); //For a document in protected view first match is not the window with the workbook. 
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
				log.Debug("Plugin - JobCTRL.Excel: Error while capturing DocumentInfo from Excel.", ex);
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
			object worksheet = null, workbook = null;
			try
			{
				worksheet = nativeObj.GetType().InvokeMember("ActiveSheet", BindingFlags.GetProperty, null, nativeObj, null);
				workbook = worksheet.GetType().InvokeMember("Parent", BindingFlags.GetProperty, null, worksheet, null);
				var fullname = workbook.GetType().InvokeMember("FullName", BindingFlags.GetProperty, null, workbook, null, ci);	//Passing in ci is a workaround for an issue with English Excel and non-English Windows. (http://support2.microsoft.com/default.aspx?scid=kb;en-us;320369)
				return (string)fullname;
			}
			catch (Exception ex)
			{
				log.Debug("Unable to get path", ex);
				//Debug.Print(ex.Message);
			}
			finally
			{
				ComHelper.Release(ref worksheet);
				ComHelper.Release(ref workbook);
			}

			return null;
		}

	}
}
