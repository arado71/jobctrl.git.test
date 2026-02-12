using System;
using System.Runtime.Serialization;
using log4net;

namespace JCAutomation.Data

{
	public enum CaptureType
	{
		Manual,
		Automatic,
	}

	[Serializable]
	public class CaptureInfo
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public DateTime Timestamp { get; set; }
		public string Notes { get; set; }
		public ControlInfo ActiveControl { get; set; }
		public CaptureType Type { get; set; }

		public CaptureInfo(ControlInfo controlGraph, string notes)
		{
			ActiveControl = controlGraph;
			Notes = notes;
			Timestamp = DateTime.UtcNow;
		}

		public string GetFileName()
		{
			var windowInfo = ActiveControl.GetWindowInfo();
			return Timestamp.ToString("yy-MM-dd_HH-mm-ss - " + (windowInfo != null ? windowInfo.ProcessName : ""));
		}

		[IgnoreDataMember]
		public string DisplayCaption { get { return string.IsNullOrEmpty(Notes) ? GetFileName() : Notes; } }
	}
}
