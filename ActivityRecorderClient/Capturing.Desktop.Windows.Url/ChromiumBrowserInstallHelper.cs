using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	class ChromiumBrowserInstallHelper : ChromiumInstallHelperBase
	{
		public ChromiumBrowserInstallHelper(): base(null) { }
		protected override string ExtensionRegKey => throw new NotImplementedException();

		protected override string ExtensionForceRegKey => throw new NotImplementedException();

		protected override string NativeHostRegKey => throw new NotImplementedException();

		protected override string PreferencesPath => throw new NotImplementedException();

		protected override string ImageName => throw new NotImplementedException();

		protected override Func<ChromiumCaptureClientWrapperBase> ClientWrapperFactory => throw new NotImplementedException();

		//public Chrom
	}
}
