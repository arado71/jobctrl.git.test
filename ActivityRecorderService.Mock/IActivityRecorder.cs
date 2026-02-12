using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderService
{
	[ServiceContract]
	public interface IActivityRecorder
	{
#if !DEBUG
		[OperationContract]
		void AddWorkItem(WorkItem workItem);
#endif
		[OperationContract]
		void AddWorkItemEx(WorkItem workItem);

		[OperationContract]
		ClientMenu GetClientMenu(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		string SetClientMenu(int userId, ClientMenu newMenu);

		[OperationContract]
		ClientSetting GetClientSettings(int userId, string oldVersion, out string newVersion);

#if !DEBUG //legacy method (from 2011 Sept 2) will be removed later
		[OperationContract]
		WorkTimeStats GetTodaysWorkTimeStats(int userId);
#endif

		[OperationContract]
		TotalWorkTimeStats GetTotalWorkTimeStats(int userId);

		[OperationContract]
		AuthData Authenticate(string clientInfo);

		[OperationContract]
		string GetAuthTicket(int userId);

		[OperationContract]
		List<WorkDetectorRule> GetClientRules(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		string SetClientRules(int userId, List<WorkDetectorRule> newRules);

		[OperationContract]
		List<CensorRule> GetClientCensorRules(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		string SetClientCensorRules(int userId, List<CensorRule> newRules);

		[OperationContract]
		void AddManualWorkItem(ManualWorkItem manualWorkItem);

		[OperationContract]
		void StartWork(int userId, int workId, int computerId, DateTime createDate, DateTime sendDate);

		[OperationContract]
		void StopWork(int userId, int computerId, DateTime createDate, DateTime sendDate);

		[OperationContract]
		ClientWorkTimeStats GetClientWorkTimeStats(int userId);

		[OperationContract]
		void ReportClientVersion(int userId, int computerId, int major, int minor, int build, int revision);

		[OperationContract]
		ClientComputerKick GetPendingKick(int userId, int computerId);

		[OperationContract]
		void ConfirmKick(int userId, int computerId, int kickId, KickResult result);
	}
}