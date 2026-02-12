using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	public static class DomCaptureHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string LoadResourceNoThrow(string scriptFileName)
		{
			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "Tct.ActivityRecorderClient.Capturing.Mail.Scripts." + scriptFileName;
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					if (stream == null)
					{
						log.ErrorAndFail(scriptFileName + " resource not found");
						return null;
					}
					using (var reader = new StreamReader(stream))
					{
						return reader.ReadToEnd();
					}
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to load " + scriptFileName, ex);
				return null;
			}
		}

		private static string gmailJsFrom;
		private static string gmailJsTo;
		private static string gmailJsSubject;
        private static string gmailJsId;
        private static string gmailJsJcId;

		public static string GmailJsFrom { get { return gmailJsFrom ?? (gmailJsFrom = LoadResourceNoThrow("GmailFrom.js")); } }
		public static string GmailJsTo { get { return gmailJsTo ?? (gmailJsTo = LoadResourceNoThrow("GmailTo.js")); } }
		public static string GmailJsSubject { get { return gmailJsSubject ?? (gmailJsSubject = LoadResourceNoThrow("GmailSubject.js")); } }
        public static string GmailJsId { get { return gmailJsId ?? (gmailJsId = LoadResourceNoThrow("GmailId.js")); } }
        public static string GmailJsJcId { get { return gmailJsJcId ?? (gmailJsJcId = LoadResourceNoThrow("GmailJcId.js")); } }
		public static string GmailUrl { get { return "https://mail.google.com/mail"; } }
		public static string GmailJCIdSetting { get { return "(function () {{localStorage.setItem(\"AddJCID\",{0}); localStorage.setItem(\"AddToSubj\",{1}); return localStorage.getItem(\"AddJCID\")+\" \" +localStorage.getItem(\"AddToSubj\");}})()"; } }

	}
}
