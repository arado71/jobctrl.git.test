using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient.Update
{
	public class AutoUpdateManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IUpdateService updateService = Platform.Factory.GetUpdateService();

		private bool firstTimeUpdateCheckCompleted;

		public event EventHandler<SingleValueEventArgs<bool>> NewVersionInstalledEvent;

		public AutoUpdateManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval { get { return 120000; } }

		protected override void ManagerCallbackImpl()
		{
			string method = "";
			try
			{
				method = "UpdateIfApplicable";
				var result = updateService.UpdateIfApplicable();
				log.Debug("Update service result: " + result);
				if (result.HasValue && result.Value)
				{
					method = "invoke NewVersionInstalled";
					OnNewVersionInstalledEvent(firstTimeUpdateCheckCompleted);
				}

				if (result.HasValue)
					firstTimeUpdateCheckCompleted = true;
			}
			catch (Exception ex)
			{
				log.Debug("Unable to " + method, ex);
			}
		}

		private void OnNewVersionInstalledEvent(bool firstTimeUpdateCheckCompleted)
		{
			var handler = NewVersionInstalledEvent;
			if (handler != null) handler(this, SingleValueEventArgs.Create(firstTimeUpdateCheckCompleted));
		}

		public IUpdateService UpdateService
		{
			get { return updateService; }
		}
	}
}
