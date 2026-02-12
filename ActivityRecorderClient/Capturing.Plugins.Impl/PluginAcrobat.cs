using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using System.Diagnostics;
using AcrobatAccessLib;
using log4net;
//using Accessibility;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginAcrobat : PluginOfficeBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public PluginAcrobat()
		{
			Id = "JobCTRL.Acrobat";
			ProcessNamesToCheck.Add("acrord32.exe");
			ProcessNamesToCheck.Add("acrobat.exe");
			ChildWindowPredicate = w => w.ClassName == "AVL_AVView" && w.Caption == "AVPageView";
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (ProcessNamesToCheck != null && !ProcessNamesToCheck.Contains(processName))
			{
				return null; //wrong process name
			}

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);
			log.Verbose("acrobat found");
			IDispatch nativeObj = null;
			try
			{
				var child = EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, ChildWindowPredicate);
				
				if (child == null) return null;
				log.Verbose("Child found");

				nativeObj = GetNativeObjectFromWindow(child.Handle);
				if (nativeObj == null) return null;
				log.Verbose("native object found");
				string documentFullName = ExtractDocumentFullNameFromNativeObject(nativeObj);
				log.Verbose("docfullname found: " + documentFullName);
				//IAccessible accClient = GetAccessibleObjectFromWindow(child.Handle);
				//if (accClient == null) return null;
				//string documentFullName = ExtractDocumentFullNameFromAccesibleObject(accClient);

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
				var lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
				log.Debug($"Plugin - JobCTRL.Acrobat: Error while capturing DocumentInfo from Acrobat. (lastError: {lastError:X})", ex);
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
			IPDDomDocument doc = (IPDDomDocument)nativeObj;
			string documentPath;
			int nPages;
			int firstVisiblePage;
			int lastVisiblePage;
			int status;
			string lang;
			doc.GetDocInfo(out documentPath, out nPages, out firstVisiblePage, out lastVisiblePage, out status, out lang);

			////Late Binding version does not work
			//object[] args = new object[] { null, null, null, null, null, null };
			//nativeObj.GetType().InvokeMember("GetDocInfo", System.Reflection.BindingFlags.InvokeMethod, null, nativeObj, args); // Throws COMException with TYPE_E_LIBNOTREGISTERED. maybe some istallation problem with Adobe Acrobat and Adobe Reader. (http://www.justskins.com/forums/sdk-vb-examples-jsofindwordvb-71094.html)
			//string documentPath = (string)args[0];

			documentPath = FormatPath(documentPath);
			return documentPath;
		}

		//private string ExtractDocumentFullNameFromAccesibleObject(IAccessible accClient)
		//{
		//    string documentFullName = accClient.accDescription[0];	//The description contains the full path name of the document and the number of pages it contains: “fileName, XXX pages”.
		//    if (documentFullName != null && documentFullName.LastIndexOf(',') >= 0)
		//    {
		//        documentFullName = documentFullName.Remove(documentFullName.LastIndexOf(','));
		//    }
		//    return documentFullName;
		//}

		private static string FormatPath(string path)
		{
			//TODO: Check other platforms
			if (Regex.IsMatch(path, @"^/\w/.*")) //First letter will be the drive 
			{
				path = path.Remove(0, 1).Insert(1, Path.VolumeSeparatorChar.ToString(CultureInfo.InvariantCulture));
				return Path.GetFullPath(path);
			}

			return "\\" + path.Replace('/', '\\');	//Treat as a network path
		}
	}
}
