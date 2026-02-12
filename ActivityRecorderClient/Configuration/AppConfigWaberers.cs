using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigWaberers || DEBUG

	public class AppConfigWaberers : AppConfigLiveAbstract
	{
		private static readonly Assembly assembly = Assembly.GetAssembly(typeof(AppConfig));
		private static readonly bool isPerMachineInstalled = assembly?.Location != null && assembly.Location.Contains("Program Files");

		public override bool IsRoamingStorageScopeNeeded => isPerMachineInstalled || base.IsRoamingStorageScopeNeeded;
		public override string AppClassifier => "Waberer's";
	}

#endif
}
