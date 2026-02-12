using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderService.Kicks;
using Tct.ActivityRecorderService.OnlineStats;
using Tct.ActivityRecorderService.Stats;
using Tct.ActivityRecorderService.TODOs;

namespace Tct.ActivityRecorderService
{
	[ServiceContract(Namespace = "Tct.ActivityRecorderService.IActivityMobile")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface IActivityMobile
	{
		[OperationContract]
		ClientMenu GetClientMenu(int userId, string oldVersion, out string newVersion);

#if !DEBUG //legacy method (from 2011 Nov 15) removed in 2012 Oct
		//[OperationContract]
		//WorkTimeStats GetTodaysWorkTimeStats(int userId);
#endif

#if !DEBUG //legacy method (from 2012 Oct 15)
		[OperationContract]
		TotalWorkTimeStats GetTotalWorkTimeStats(int userId);
#endif

		[OperationContract]
		ClientWorkTimeStats GetClientWorkTimeStats(int userId);

		[OperationContract]
		SimpleWorkTimeStats GetSimpleWorkTimeStats(int userId, DateTime? desiredEndDate);

		[OperationContract]
		ClientComputerKick GetPendingKick(int userId, long deviceId);

		[OperationContract]
		void ConfirmKick(int userId, long deviceId, int kickId, KickResult result);

		[OperationContract]
		void MakeClientActive(int userId, long deviceId, bool isActive);

		[OperationContract]
		List<BriefUserStats> GetBriefUserStats(List<int> userIds);

		[OperationContract]
		List<DetailedUserStats> GetDetailedUserStats(List<int> userIds);

		[XmlSerializerFormat]
		[OperationContract]
		TodoListDTO GetTodoList(int userId, DateTime date);

		[XmlSerializerFormat]
		[OperationContract]
		List<TodoListItemStatusDTO> GetTodoListItemStatuses();

		[XmlSerializerFormat]
		[OperationContract]
		bool CreateOrUpdateTodoList(TodoListDTO todoList);

		[XmlSerializerFormat]
		[OperationContract]
		TodoListDTO GetMostRecentTodoList(int userId);

		[XmlSerializerFormat]
		[OperationContract]
		TodoListToken AcquireTodoListLock(int userId, int todoListId);

		[XmlSerializerFormat]
		[OperationContract]
		bool ReleaseTodoListLock(int userId, int todoListId);
	}
}
