using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	public class StatsFilter
	{
		public readonly HashSet<int> UserIds;
		public readonly int? GroupId;
		public readonly int? CompanyId;

		public StatsFilter(IEnumerable<int> userIds, int? groupId, int? companyId)
		{
			if (userIds != null)
			{
				UserIds = new HashSet<int>(userIds);
			}
			GroupId = groupId;
			CompanyId = companyId;
		}
	}
}
