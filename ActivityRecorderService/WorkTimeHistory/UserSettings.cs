using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.WorkTimeHistory
{
	public class UserSettings
	{
		public bool IsModificationApprovalNeeded { get; set; }
		public int ModificationAgeLimitInHours { get; set; }

		public TimeSpan ModificationAgeLimit
		{
			get { return TimeSpan.FromHours(ModificationAgeLimitInHours); }
		}

		public override string ToString()
		{
			return "approveNeed: " + IsModificationApprovalNeeded.ToString() + " modLimitHrs: " + ModificationAgeLimitInHours.ToInvariantString();
		}
	}
}
