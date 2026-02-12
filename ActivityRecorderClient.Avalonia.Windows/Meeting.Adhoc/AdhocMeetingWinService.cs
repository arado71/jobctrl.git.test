using System;
using System.Diagnostics;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View;
using Tct.ActivityRecorderClient.View.Presenters;

namespace Tct.ActivityRecorderClient.Meeting.Adhoc
{
	public class AdhocMeetingWinService : AdhocMeetingService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Form owner;
		private OfflineWorkForm form;


		public AdhocMeetingWinService(Form owner, CaptureCoordinator captureCoordinator)
			: base(captureCoordinator)
		{
			Debug.Assert(owner is ActivityRecorderForm);
			this.owner = owner;
		}

		protected override OfflineWorkPresenter OpenGui()
		{
			form = new OfflineWorkForm(this, owner);
			return form.Presenter;
		}

	}
}
