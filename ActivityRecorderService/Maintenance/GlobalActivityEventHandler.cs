using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tct.ActivityRecorderService.Maintenance
{
	public static class GlobalActivityEventDbHelper
	{
		public static void Add(GlobalActivityEventType eventType, string name)
		{
			var activityEvent = eventType == GlobalActivityEventType.ComponentStarted ? (GlobalActivityEvent)new ComponentStartedEvent { ComponentName = name } : eventType == GlobalActivityEventType.ComponentStopped ? new ComponentStoppedEvent { ComponentName = name } : throw new ArgumentException($"GlobalActivityEventType: {eventType} not implemented");
			var json = JsonConvert.SerializeObject(activityEvent);
			using (var context = new JobControlDataClassesDataContext())
				context.InsertGlobalActivityEvent((int)eventType, json);
		}
	}
}
