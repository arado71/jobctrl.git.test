using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tct.ActivityRecorderService.Maintenance
{
	public abstract class GlobalActivityEvent
	{
		[JsonIgnore]
		public DateTime CreatedAt;
	}

	public class ComponentStartedEvent : GlobalActivityEvent
	{
		public string ComponentName;
	}

	public class ComponentStoppedEvent : GlobalActivityEvent
	{
		public string ComponentName;
	}

	public enum GlobalActivityEventType
	{
		ComponentStarted = 1,
		ComponentStopped = 2,
		JobStarted = 3,
		JobStopped = 4,
	}

}
