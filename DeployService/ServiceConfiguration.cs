using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using log4net;

namespace Tct.DeployService
{
	public class ServiceConfiguration
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ServiceConfiguration));

		public static ServiceConfiguration Load(string file)
		{
			var xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(file);
			}
			catch (Exception e)
			{
				logger.ErrorFormat("Error while processing configuration file {0}: {1}", file, e.Message);
				return null;
			}

			var res = new ServiceConfiguration
			{
				RecorderDatabase = DatabaseConfiguration.Parse(GetAttribute(xmlDoc,
					"//add[@name=\"Tct.ActivityRecorderService.Properties.Settings.recorderConnectionString\"]", "connectionString")),
				JobCtrlDatabase = DatabaseConfiguration.Parse(GetAttribute(xmlDoc,
					"//add[@name=\"Tct.ActivityRecorderService.Properties.Settings._jobcontrolConnectionString\"]", "connectionString")),
				IvrDatabase = DatabaseConfiguration.Parse(GetAttribute(xmlDoc,
					"//add[@name=\"Tct.ActivityRecorderService.Properties.Settings.jobcontrolivrConnectionString\"]",
					"connectionString")),
				ScreenshotDir = GetAttribute(xmlDoc, "//add[@key=\"ScreenShotsDir\"]", "value"),
				EmailsDir = GetAttribute(xmlDoc, "//add[@key=\"EmailsToSendDir\"]", "value"),
				DeadLetterDir = GetAttribute(xmlDoc, "//add[@key=\"DeadLetterDir\"]", "value"),
				VoiceRecDir = GetAttribute(xmlDoc, "//add[@key=\"VoiceRecordingsDir\"]", "value"),
				MobileUpdateInterval = Parse(GetAttribute(xmlDoc, "//add[@key=\"MobileStatusUpdateInterval\"]", "value"), -1),
				Smtp = new SmtpConfiguration()
				{
					Address = GetAttribute(xmlDoc, "//add[@key=\"EmailFrom\"]", "value"),
					Host = GetAttribute(xmlDoc, "//add[@key=\"EmailSmtpHost\"]", "value"),
					Password = GetAttribute(xmlDoc, "//add[@key=\"EmailPassword\"]", "value"),
					Port = Parse(GetAttribute(xmlDoc, "//add[@key=\"EmailSmtpPort\"]", "value"), 23),
					Ssl = Parse(GetAttribute(xmlDoc, "//add[@key=\"EmailSsl\"]", "value"), false),
					User = GetAttribute(xmlDoc, "//add[@key=\"EmailUserName\"]", "value")
				}
			};

			return res;
		}

		private static int Parse(string str, int defaultValue)
		{
			int n;
			return int.TryParse(str, out n) ? n : defaultValue;
		}

		private static bool Parse(string str, bool defaultValue)
		{
			bool b;
			return bool.TryParse(str, out b) ? b : defaultValue;
		}

		private static string GetAttribute(XmlDocument doc, string xpath, string attribute)
		{
			var node =
				doc.SelectSingleNode(xpath);
			return node != null && node.Attributes != null && node.Attributes[attribute] != null
				? node.Attributes[attribute].InnerText
				: null;
		}

		public DatabaseConfiguration RecorderDatabase { get; set; }
		public DatabaseConfiguration JobCtrlDatabase { get; set; }
		public DatabaseConfiguration IvrDatabase { get; set; }

		public string ScreenshotDir { get; set; }
		public string EmailsDir { get; set; }
		public string DeadLetterDir { get; set; }
		public string VoiceRecDir { get; set; }
		public int MobileUpdateInterval { get; set; }
		public SmtpConfiguration Smtp { get; set; }

		public string WebsiteApiAddress { get; set; }

	}
}
