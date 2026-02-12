using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome
{
	public class EdgeBlinkInstallHelper : ChromiumInstallHelperBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Lazy<EdgeBlinkInstallHelper> lazyInstance = new Lazy<EdgeBlinkInstallHelper>(() => new EdgeBlinkInstallHelper());

		protected override string ExtensionRegKey => @"Software\Microsoft\Edge\Extensions\";
		protected override string ExtensionForceRegKey => @"Software\Policies\Microsoft\Edge\ExtensionInstallForcelist\";
		protected override string NativeHostRegKey => @"Software\Microsoft\Edge\NativeMessagingHosts\com.tct.jobctrl";
		protected override string PreferencesPath => @"Microsoft\Edge\User Data\Default\Preferences";
		protected override string ImageName => "msedge";
		protected override Func<ChromiumCaptureClientWrapperBase> ClientWrapperFactory => () => new EdgeBlinkCaptureClientWrapper();

		public EdgeBlinkInstallHelper() : base(log)
		{
		}

		public static void InstallExtensionOneTimeIfApplicable()
		{
			lazyInstance.Value.InstallExtensionOneTimeIfApplicableInternal();
		}

	}
}
