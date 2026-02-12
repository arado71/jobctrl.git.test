using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient;
using VoxCTRL.Controller;
using VoxCTRL.Voice;

namespace VoxCTRL.Update
{
	public class UpdateManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int updateInterval = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;

		private readonly UpdateService service = new UpdateService();
		private readonly SynchronizationContext context;

		//properties are only accessed on the GUI thread
		private bool CanPerformUpdate { get; set; }
		private bool IsUpdateAvailable { get; set; }

		public UpdateManager(SynchronizationContext guiContext, RecorderFormController controller)
			: base(log)
		{
			context = guiContext;
			service.Initialize();
			controller.PropertyChanged += (_, e) => { if (e.PropertyName == "State") SetState(controller.State); };
			SetState(controller.State);
		}

		public string GetAppPath()
		{
			return service.GetAppPath();
		}

		protected override int ManagerCallbackInterval
		{
			get { return updateInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			if (service.UpdateIfApplicable())
			{
				context.Post(_ =>
				{
					log.Info("Update is available");
					IsUpdateAvailable = true;
					UpdateIfApplicable();
				}, null);
			}
		}

		private void SetState(RecordingState state)
		{
			CanPerformUpdate = state == RecordingState.Stopped;
			UpdateIfApplicable();
		}

		private void UpdateIfApplicable()
		{
			if (IsUpdateAvailable && CanPerformUpdate)
			{
				log.Info("Installing update and restarting");
				service.RestartWithNewVersion();
			}
		}
	}
}
