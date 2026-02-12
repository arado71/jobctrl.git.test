using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using log4net;

namespace Tct.ActivityRecorderService.Telemetry
{
	public static class TelemetryHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Save(TelemetryItem telemetry)
		{
			int groupId, companyId;
			if (!UserIdManager.Instance.TryGetIdsForUser(telemetry.UserId, out groupId, out companyId))
			{
				//don't need to log this as error so it is outside of the try block
				log.Info("Save stats failed " + telemetry.UserId + " user is not active");
				throw new FaultException("User is not active");
			}

			var dir = Path.Combine(ConfigManager.TelemetryDataDir, companyId.ToInvariantString(), telemetry.UserId.ToInvariantString());
			var filepath = Path.Combine(dir, Guid.NewGuid() + ".jtd");
			Directory.CreateDirectory(dir);
			SaveToFile(telemetry, filepath);
		}

		private static void SaveToFile(TelemetryItem telemetry, string filepath)
		{
			using (var stream = File.Create(filepath))
			{
				telemetry.WriteTo(stream);
			}
		}
	}
}
