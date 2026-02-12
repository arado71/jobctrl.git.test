using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome
{
	public class ChromeInstallHelper : ChromiumInstallHelperBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Lazy<ChromeInstallHelper> lazyInstance = new Lazy<ChromeInstallHelper>(()=> new ChromeInstallHelper());

		protected override string ExtensionRegKey => @"Software\Google\Chrome\Extensions\";
		protected override string ExtensionForceRegKey => @"Software\Policies\Google\Chrome\ExtensionInstallForcelist\";
		protected override string NativeHostRegKey => @"Software\Google\Chrome\NativeMessagingHosts\com.tct.jobctrl";
		protected override string PreferencesPath => @"Google\Chrome\User Data\Default\Preferences";
		protected override string ImageName => "chrome";
		protected override Func<ChromiumCaptureClientWrapperBase> ClientWrapperFactory => () => new ChromeCaptureClientWrapper();

		public ChromeInstallHelper() : base(log)
		{
		}

		public static void InstallExtensionOneTimeIfApplicable()
		{
			lazyInstance.Value.InstallExtensionOneTimeIfApplicableInternal();
		}

	}
}
