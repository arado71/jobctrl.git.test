using ActivityRecorderClientAV;
using log4net;
using Tct.ActivityRecorderClient.Avalonia.UI.Views;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.View.Presenters;

namespace Tct.ActivityRecorderClient.Mac.Meeting.Adhoc
{
	public class AdhocMeetingMacService : AdhocMeetingService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private OfflineWorkForm form;

		public AdhocMeetingMacService(CaptureCoordinator captureCoordinator)
			: base(captureCoordinator)
		{
		}

		protected override OfflineWorkPresenter OpenGui()
		{
			form = new OfflineWorkForm(this);
			return form.Presenter;
		}

	}
}
