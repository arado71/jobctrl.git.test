using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigDefault || DEBUG

	public class AppConfigDefault : AppConfigLiveAbstract
	{
	}

#endif
}
