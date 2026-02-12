using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Notification
{
	public static class NotificationKeys
	{
		private static readonly Dictionary<string, bool> importantKeys = new Dictionary<string, bool>() { //HashSet is not documented as thread-safe for concurrent readers but Dictionary is
				{ NoWork, true },
				{ InvalidTimeCannotWork, true },
				{ CreditRunOut, true },
				{ PersistAndSendError, true },
			};

		public const string NoWork = "NoWork";
		public const string NotWorking = "NotWorking";
		public const string MenuChange = "MenuChange";
		public const string InvalidWork = "InvalidWork";
		public const string NewWork = "NewWork";
		public const string MoreWorksInCat = "MoreWorksInCat";
		public const string EmergencyRestart = "EmergencyRestart";

		public const string InvalidUserRules = "InvalidUserRule";
		public const string InvalidTimeCannotWork = "InvalidTimeCannotWork";
		public const string CreditRunOut = "CreditRunOut";

		public const string IdleStopWork = "IdleStopWork";
		public const string PersistAndSendError = "PersistAndSendError";
		public const string ActiveOnly = "ActiveOnly";
		public const string AssignTaskError = "AssignTaskError";
		public const string DisplayNewFeatures = "DisplayNewFeatures";

		public const string KickStopWork = "KickStopWork";

		public const string AssignWorkNotAllowed = "AssignWorkNotAllowed";

		public const string ClockSkew = "ClockSkew";

		public const string WmsWarn = "WMSWarn";

		public const string UpdateFailed = "UpdateFailed";

		public const string RulesChanged = "RulesChanged";

		public const string TaskSwitchNotPossible = "TaskSwitchNotPossible";

		public const string TimeZoneNotSet = "TimeZoneNotSet";

		public const string GoogleCredential = "GoogleCredential";

		public static bool IsImportant(string key)
		{
			return importantKeys.ContainsKey(key);
		}
	}
}
