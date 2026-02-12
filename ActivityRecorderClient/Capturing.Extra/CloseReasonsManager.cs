using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Extra
{
	/// <summary>
	/// Class for updating canned close reasons
	/// </summary>
	public class CloseReasonsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int callbackInterval = 8 * 60 * 60 * 1000;  //8 hours
		private const int callbackRetryInterval = 60 * 1000;  //60 secs
		private static string FilePath { get { return "CloseReasons-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<CannedCloseReasons>> CannedCloseReasonsChanged;

		private CannedCloseReasons cannedCloseReasons = new CannedCloseReasons();
		private CannedCloseReasons CannedCloseReasons
		{
			get { return cannedCloseReasons; } //we don't need to expose this atm. (and it's not thread-safe)
			set
			{
				if (value == null) //cannot save null value
				{
					CannedCloseReasons = new CannedCloseReasons();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(cannedCloseReasons, value)) return;
				log.Info("Canned Close Reasons changed");
				cannedCloseReasons = value;
				IsolatedStorageSerializationHelper.Save(FilePath, value);
				OnCannedCloseReasonsChanged(value);
			}
		}

		private bool lastSendFailed;

		public CloseReasonsManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get { return lastSendFailed ? callbackRetryInterval : callbackInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				var reasons = ActivityRecorderClientWrapper.Execute(n => n.GetCannedCloseReasons(userId));
				lastSendFailed = false;
				CannedCloseReasons = reasons;
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get Canned Close Reasons", log, ex);
				lastSendFailed = true; //retry shortly
			}
		}

		public void LoadData()
		{
			log.Info("Loading Canned Close Reasons from disk");
			CannedCloseReasons value;
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
				&& IsolatedStorageSerializationHelper.Load(FilePath, out value))
			{
				cannedCloseReasons = value;
			}
			OnCannedCloseReasonsChanged(cannedCloseReasons); //always raise so we know the initial state
		}

		private void OnCannedCloseReasonsChanged(CannedCloseReasons value)
		{
			Debug.Assert(value != null);
			var del = CannedCloseReasonsChanged;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(value));
		}
	}
}