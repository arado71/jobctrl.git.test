using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigJC360Lite || DEBUG

	public class AppConfigJC360Lite : AppConfigLiveAbstract
	{
		public override string TaskPlaceholder => "JC360HomeOffice";
		public override string AppNameOverride => "JC360 HomeOffice";
		public override bool DisplaySummaDelta => false;
	}

#endif
}
