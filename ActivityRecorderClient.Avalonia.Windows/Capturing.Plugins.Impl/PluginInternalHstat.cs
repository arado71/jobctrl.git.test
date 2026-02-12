using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Extra;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginInternalHStat : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "Internal.HStat";
		public const string ParamQueryFilter = "QueryFilter";
		public const string KeyResult = "Result";
		private const int queryInterval = 5 * 60 * 1000; // 5mins
		private string cachedQueryResult;

		private QueryFilter queryFilter;
		private int lastQueryTime;

		public string Id => PluginId;

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamQueryFilter;
		}

		public void SetParameter(string name, string value)
		{
			if (name != ParamQueryFilter) return;
			if (int.TryParse(value, out var queryFilterInt))
			{
				queryFilter = (QueryFilter)queryFilterInt;
			}
			else
			{
				log.Warn("QueryFilter value is not numeric: " + value);
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyResult;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (processName != "explorer.exe") return null;
			var now = Environment.TickCount;
			if (now - lastQueryTime < queryInterval && now > lastQueryTime) return new []{ new KeyValuePair<string, string>(KeyResult, cachedQueryResult) };
			lastQueryTime = now;
			try
			{
				string filter = null;
				if (queryFilter.HasFlag(QueryFilter.MouseDevice)) filter = "PnpClass = 'Mouse'";
				if (queryFilter.HasFlag(QueryFilter.KeyboardDevice)) filter = (filter != null ? filter + " Or " : "") + "PnpClass = 'Keyboard'";
				var devices = UsbDevices.GetUSBDevices(filter);
				devices.Sort();
				cachedQueryResult = string.Join(";", devices.Select(f => "{" + f + "}"));
				return new[] { new KeyValuePair<string, string>(KeyResult, cachedQueryResult) };
			}
			catch (Exception ex)
			{
				log.Error("Query of devices failed", ex);
				return null;
			}
		}

		[Flags]
		private enum QueryFilter
		{
			KeyboardDevice = 1,
			MouseDevice = 2,
		}
	}
}
