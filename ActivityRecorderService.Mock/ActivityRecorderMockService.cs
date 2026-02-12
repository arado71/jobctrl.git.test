using System;
using System.Collections.Generic;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace ActivityRecorderService.Mock
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class ActivityRecorderMockService : Tct.ActivityRecorderService.IActivityRecorder
	{
		public static Version Version { get { return new Version(); } }

		#region IActivityRecorder implementation
		public void AddWorkItemEx(WorkItem workItem)
		{
			Console.WriteLine("AddWorkItemEx: " + GetUserId() + " " + workItem.StartDate + " m:" + workItem.MouseActivity + " k:" + workItem.KeyboardActivity);
		}

		public ClientMenu GetClientMenu(int userId, string oldVersion, out string newVersion)
		{
			var currVer = "asd";//DateTime.Now.Minute.ToString();
			newVersion = currVer;
			if (oldVersion == currVer)
			{
				return null;
			}
			return new ClientMenu() { Works = new List<WorkData>()
				{
					new WorkData() { Id = 1, Name = "bla" },
					new WorkData() { Name = "Proj", Children = new List<WorkData>() { new WorkData() { Id = 2, Name = "bla2" }, } },
					new WorkData() { Id = DateTime.Now.Minute, Name = "bla" + DateTime.Now.Minute.ToString(), },
				}
			};
		}

		public string SetClientMenu(int userId, ClientMenu newMenu)
		{
			throw new NotImplementedException();
		}

		public ClientSetting GetClientSettings(int userId, string oldVersion, out string newVersion)
		{
			newVersion = oldVersion;
			return null;
		}

		public TotalWorkTimeStats GetTotalWorkTimeStats(int userId)
		{
			return new TotalWorkTimeStats() {
				FromDate = DateTime.MinValue,
				ToDate = DateTime.UtcNow,
				UserId = userId,
				Stats = new Dictionary<int, TotalWorkTimeStat>() {
					{ 1, new TotalWorkTimeStat() { WorkId = 1, ComputerWorkTime = TimeSpan.FromDays(3.45), TotalWorkTime =  TimeSpan.FromDays(3.45), }}
				}
			};
		}

		public AuthData Authenticate(string clientInfo)
		{
			return new AuthData() { Name = "Z", Id = GetUserId(), Email = "valami@valami.hu" };
		}

		public string GetAuthTicket(int userId)
		{
			throw new NotImplementedException();
		}

		public List<WorkDetectorRule> GetClientRules(int userId, string oldVersion, out string newVersion)
		{
			newVersion = oldVersion;
			return null;
		}

		public string SetClientRules(int userId, List<WorkDetectorRule> newRules)
		{
			throw new NotImplementedException();
		}

		public List<CensorRule> GetClientCensorRules(int userId, string oldVersion, out string newVersion)
		{
			newVersion = oldVersion;
			return null;
		}

		public string SetClientCensorRules(int userId, List<CensorRule> newRules)
		{
			throw new NotImplementedException();
		}

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			throw new NotImplementedException();
		}

		public void StartWork(int userId, int workId, int computerId, DateTime createDate, DateTime sendDate)
		{
		}

		public void StopWork(int userId, int computerId, DateTime createDate, DateTime sendDate)
		{
		}

		public ClientWorkTimeStats GetClientWorkTimeStats(int userId)
		{
			var time = DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromHours(7));
			if (time < TimeSpan.Zero)
				time = TimeSpan.Zero;
			return new ClientWorkTimeStats() {
				TodaysWorkTime = new BriefNetWorkTimeStats(){ ComputerWorkTime =  time, NetWorkTime = time },
				TodaysTargetNetWorkTime = TimeSpan.FromHours(8),
				ThisWeeksWorkTime = new BriefNetWorkTimeStats(){ ComputerWorkTime =  time + TimeSpan.FromHours(8), NetWorkTime = time + TimeSpan.FromHours(8),},
				ThisWeeksTargetNetWorkTime = TimeSpan.FromHours(40),
				ThisWeeksTargetUntilTodayNetWorkTime =TimeSpan.FromHours(16),
				ThisMonthsWorkTime = new BriefNetWorkTimeStats(){ ComputerWorkTime =  time + TimeSpan.FromHours(48), NetWorkTime = time + TimeSpan.FromHours(48),},
				ThisMonthsTargetNetWorkTime = TimeSpan.FromHours(168),
				ThisMonthsTargetUntilTodayNetWorkTime =TimeSpan.FromHours(56),
			};

		}

		public void ReportClientVersion(int userId, int computerId, int major, int minor, int build, int revision)
		{
		}

		public ClientComputerKick GetPendingKick(int userId, int computerId)
		{
			return null;
		}

		public void ConfirmKick(int userId, int computerId, int kickId, KickResult result)
		{
			throw new NotImplementedException();
		}
		#endregion

		//http://mono.1490590.n4.nabble.com/WCF-ServiceSecurityContext-td3307343.html
		private static int GetUserId()
		{
			object propObj;

			OperationContext.Current.IncomingMessageProperties.TryGetValue(HttpRequestMessageProperty.Name, out propObj);

			HttpRequestMessageProperty reqProp = (HttpRequestMessageProperty)propObj;
			string headerAuth = reqProp.Headers["Authorization"];
			if (headerAuth.StartsWith("Basic "))
			{
				return int.Parse(Encoding.ASCII.GetString(Convert.FromBase64String(headerAuth.Substring(6))).Split(new [] {':'})[0]);
			}

			//this doesn't work on Mono (it is null)
			var userIdStr = ServiceSecurityContext.Current.PrimaryIdentity.Name;
			return int.Parse(userIdStr);
		}
	}
}

