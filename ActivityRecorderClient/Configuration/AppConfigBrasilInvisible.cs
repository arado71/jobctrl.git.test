using System.Windows.Forms;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigBrasilInvisible || DEBUG

	public class AppConfigBrasilInvisible : AppConfigBrasilBase
	{
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "Brasil/Invisible";
		public override bool IsTaskBarIconShowing => false;
		public override NotificationPosition NotificationPosition => NotificationPosition.Hidden;
		public override Keys? ManualMeetingHotKey => new Keys?();
		public override bool SuppressActiveDirectoryFallbackLogin => true;
		public override bool AutoUpdateManagerEnabled => false;
		public override bool IsRoamingStorageScopeNeeded => true;
	}

#endif
}
