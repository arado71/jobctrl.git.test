using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	/// <summary>
	/// Plugin for capturing opened file names for processes.
	/// </summary>
	public class PluginFileHandles : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "JobCTRL.File";
		private const string KeyDocumentFullName = "DocumentFullName";
		//private const string KeyDocumentPath = "DocumentPath";
		//private const string KeyDocumentFileName = "DocumentFileName";
		private const string ParamProcessName = "ProcessName";
		//private const string ParamFileMask = "FileMask";

		private HashSet<string> ProcessNamesToCheck { get; set; }

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamProcessName;
		}

		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamProcessName, StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrEmpty(value))
				{
					ProcessNamesToCheck = null;
				}
				else
				{
					ProcessNamesToCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (var file in value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
					{
						ProcessNamesToCheck.Add(file);
					}
				}
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyDocumentFullName;
			//yield return KeyDocumentPath;
			//yield return KeyDocumentFileName;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (ProcessNamesToCheck == null || !ProcessNamesToCheck.Contains(processName)) //we have to specify ProcessName param (capturing for all processes are too expensive)
			{
				return null; //wrong process name
			}

			var files = FileHandleHelper.GetOpenFiles(processId);

			return new Dictionary<string, string>(1)
			{
				{KeyDocumentFullName, files == null ? "": string.Join("|", files.ToArray()) }
			};
		}
	}
}
