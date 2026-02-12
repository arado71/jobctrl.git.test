using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginDateTime : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "JobCTRL.DateTime";
		private const string ParamDateTimeFormatString = "DateTimeFormatString";	//"Standard Date and Time Format Strings" (http://msdn.microsoft.com/en-us/library/az4se3k1) or "Custom Date and Time Format Strings" (http://msdn.microsoft.com/en-us/library/8kb3ddd4) can be used in this parameter
		private const string KeyDate = "Date";	//Custom short date pattern (yyyy-MM-dd)
		private const string KeyTime = "Time";	//Short time pattern with InvariantCulture (HH:mm)
		private const string KeyCustomDateTime = "CustomDateTime";	//Current date and time formatted with DateTimeFormatString parameter and InvariantCulture

		private string dateTimeFormatString = "";

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamDateTimeFormatString;
		}

		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamDateTimeFormatString, StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					DateTime.Now.ToString(value, DateTimeFormatInfo.InvariantInfo);
					dateTimeFormatString = value;
				}
				catch (FormatException e)
				{
					log.Warn("Invalid DateTime format string. (" + value + ")", e);
				}
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyDate;
			yield return KeyTime;
			yield return KeyCustomDateTime;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			var now = DateTime.Now;
			return new[] { 
					new KeyValuePair<string, string>(KeyDate, now.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo)),
					new KeyValuePair<string, string>(KeyTime, now.ToString("t", DateTimeFormatInfo.InvariantInfo)),
					new KeyValuePair<string, string>(KeyCustomDateTime, string.IsNullOrEmpty(dateTimeFormatString) ? "" : now.ToString(dateTimeFormatString, DateTimeFormatInfo.InvariantInfo)),
				};
		}
	}
}
