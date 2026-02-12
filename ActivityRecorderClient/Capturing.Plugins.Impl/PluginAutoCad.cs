using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	class PluginAutoCad : PluginOfficeBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string AUTOCAD_PROG_ID = "AutoCAD.Application";
		private const string AUTOCAD_ASSEMBLY_PATH = "Autodesk.AutoCAD.Interop, Version=18.2.0.0, Culture=neutral, PublicKeyToken=eed84259d7cbf30b";
		private const string AUTOCAD_APPLICATION_TYPENAME = "Autodesk.AutoCAD.Interop.IAcadApplication";
		private const string AUTOCAD_DOCUMENT_TYPENAME = "Autodesk.AutoCAD.Interop.IAcadDocument";
		private static bool isCapturingPossible = true;
		private Dictionary<string, string> lastResult = null;

		private Assembly autoCadAssembly;
		private Type acadApplicationType;
		private Type acadDocumentType;
		private PropertyInfo activeDocumentProperty;
		private PropertyInfo documentPathProperty;
		private PropertyInfo documentNameProperty;
		private PropertyInfo documentFullNameProperty;

		public PluginAutoCad()
		{
			Id = "JobCTRL.AutoCAD";
			ProcessNamesToCheck.Add("acad.exe");
			initialize();
		}

		private void initialize()
		{
			try
			{
				if (Update.ElevatedPrivilegesHelper.IsElevated)
				{
					isCapturingPossible = false;
					log.Debug("Initializing AutoCAD interop error. Maybe AutoCAD is not installed. Capturing disabled.");
					return;
				}
				autoCadAssembly = Assembly.Load(AUTOCAD_ASSEMBLY_PATH);
				acadApplicationType = autoCadAssembly.GetType(AUTOCAD_APPLICATION_TYPENAME);
				acadDocumentType = autoCadAssembly.GetType(AUTOCAD_DOCUMENT_TYPENAME);
				activeDocumentProperty = acadApplicationType.GetProperty("ActiveDocument");
				documentPathProperty = acadDocumentType.GetProperty("Path");
				documentNameProperty = acadDocumentType.GetProperty("Name");
				documentFullNameProperty = acadDocumentType.GetProperty("FullName");
			}
			catch (System.IO.FileNotFoundException)
			{
				isCapturingPossible = false;
				log.Debug("Initializing AutoCAD interop error. Maybe AutoCAD is not installed. Capturing disabled.");
			}
			catch(Exception e)
			{
				isCapturingPossible = false;
				log.Debug(e);
			}
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!isCapturingPossible || (ProcessNamesToCheck != null && !ProcessNamesToCheck.Contains(processName)))
			{
				return null; //wrong process name
			}
			try
			{
				object activeObject = Marshal.GetActiveObject(AUTOCAD_PROG_ID);
				object activeDocument = activeDocumentProperty.GetValue(activeObject, null);
				string path = documentPathProperty.GetValue(activeDocument, null) as string;
				string name = documentNameProperty.GetValue(activeDocument, null) as string;
				string fullName = documentFullNameProperty.GetValue(activeDocument, null) as string;
				var result = new Dictionary<string, string>
				{
					{KeyDocumentFullName, fullName },
					{KeyDocumentPath, path },
					{KeyDocumentFileName, name }
				};
				lastResult = result;
				return result;
			}
			catch (COMException ce)
			{
				lastResult = null;
				if (ce.ErrorCode == -2147221005)
				{
					log.Debug("AutoCAD capturing doesn't work, because the JC runs from a different user then AutoCAD");
					isCapturingPossible = false;
					return null;
				}
				// This error occurs when the autocad is initializing.
				if (ce.ErrorCode == -2147221021)
					return null;
				log.Warn("Unexpected error.", ce);
				return null;
			}
			// We don't care about these errors.
			catch (TargetInvocationException tie)
			{
				return lastResult;
			}
		}
	}
}
