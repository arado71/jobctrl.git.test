using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderService.Kicks;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService
{
	[ServiceContract(Namespace = "Tct.ActivityRecorderService.IActivityMonitoring")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface IActivityMonitoring
	{
		[OperationContract]
		List<BriefUserStats> GetBriefUserStats(List<int> userIds);

		[OperationContract]
		List<DetailedUserStats> GetDetailedUserStats(List<int> userIds);

		[OperationContract]
		[FaultContract(typeof(KickTimeoutException))]
		KickResult KickUserComputer(int userId, int computerId, string reason, TimeSpan expiration);

		[OperationContract]
		SimpleWorkTimeStats GetSimpleWorkTimeStatsById(int userId, DateTime? desiredEndDate);
	}
}
