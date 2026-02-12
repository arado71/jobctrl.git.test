using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.SoftphonePro
{
	public class SoftphoneConfigInjector
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static SoftphoneConfigInjector Instance = new SoftphoneConfigInjector();
		private readonly object lockObject = new object();
		private readonly string[] jcConfigLines = {
			string.Empty,
			"[ExternalEventReceiver1]",
			"EventType=CallFinished",
			"AppType=Web",
			"Link=\"{URL}/callfinished?number=%NUMBER%\"",
			string.Empty,
			"[ExternalEventReceiver2]",
			"EventType=IncomingCallAnswered",
			"AppType=Web",
			"Link=\"{URL}/callanswered?number=%NUMBER%\"",
			string.Empty,
			"[ExternalEventReceiver3]",
			"EventType=OutgoingCall",
			"AppType=Web",
			"Link=\"{URL}/calloutgoing?number=%NUMBER%\""
		};

		private SoftphoneConfigInjector()
		{
		}

		public void UpdateConfiguration(string baseUrl)
		{
			var localAppPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var configFile = Path.Combine(Path.Combine(localAppPath, "SoftphonePro"), "SoftphonePro.ini");

			using (StreamWriter file = new StreamWriter(configFile, true))
			{
				jcConfigLines.ToList().ForEach((line) => {
					file.WriteLine(line.Replace("{URL}", baseUrl));
				});
			}
			log.DebugFormat("SoftphonePro configuration is updated ({0})", configFile);
		}

		public bool IsConfigurationUpdateRequired(string baseUrl)
		{
			var localAppPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var configFile = Path.Combine(Path.Combine(localAppPath, "SoftphonePro"), "SoftphonePro.ini");

			if (!File.Exists(configFile))
			{
				log.DebugFormat("SoftphonePro configuration is not found ({0})", configFile);
				return false;
			}

			var content = File.ReadAllText(configFile);
			if (content.Contains(string.Join(Environment.NewLine, jcConfigLines).Replace("{URL}", baseUrl)))
			{
				log.DebugFormat("SoftphonePro configuration is OK ({0})", configFile);
				return false;
			}
			log.DebugFormat("SoftphonePro configuration update required ({0})", configFile);
			return true;
		}
	}
}
