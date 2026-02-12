using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Linq;

namespace MonitoringClient.ActivityMonitoringServiceReference
{
	public partial class BriefUserStats
	{
		private bool isSelectedForDetails;
		public bool IsSelectedForDetails
		{
			get
			{
				return this.isSelectedForDetails;
			}
			set
			{
				if ((this.isSelectedForDetails.Equals(value) != true))
				{
					this.isSelectedForDetails = value;
					this.RaisePropertyChanged("IsSelectedForDetails");
				}
			}
		}

		private string currentWorkString;
		public string CurrentWorkString
		{
			get { return currentWorkString ?? (CurrentWorkString = GetCurrentWorkString()); }
			set
			{
				if (this.currentWorkString != value)
				{
					this.currentWorkString = value;
					this.RaisePropertyChanged("CurrentWorkString");
				}
			}
		}

		private ObservableCollection<BriefWorkStats> todaysWorks;
		public ObservableCollection<BriefWorkStats> TodaysWorks
		{
			get
			{
				if (todaysWorks == null)
				{
					todaysWorks = new ObservableCollection<BriefWorkStats>();
					foreach (var briefWork in TodaysWorksByWorkId.Values)
					{
						todaysWorks.Add(briefWork);
					}
				}
				return todaysWorks;
			}
		}

		public DetailedUserStats ToDetailedUserStats()
		{
			var graph = this;
			byte[] origBytes;
			using (var stream = new MemoryStream())
			{
				using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream))
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(BriefUserStats));
					serializer.WriteObject(writer, graph);
				}
				origBytes = stream.ToArray();
			}
			using (var stream = new MemoryStream(origBytes, false))
			{
				using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, XmlDictionaryReaderQuotas.Max))
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(DetailedUserStats));
					return (DetailedUserStats)serializer.ReadObject(reader);
				}
			}
		}

		public void Update(BriefUserStats newStats)
		{
			this.Status = newStats.Status;
			this.TodaysEndDate = newStats.TodaysEndDate;
			this.TodaysStartDate = newStats.TodaysStartDate;
			this.UserName = newStats.UserName;
			this.UserId = newStats.UserId;
			this.UserTimeZoneString = newStats.UserTimeZoneString;
			this.TodaysWorkTime.NetWorkTime = newStats.TodaysWorkTime.NetWorkTime;
			this.TodaysWorkTime.Update(newStats.TodaysWorkTime);
			this.ThisWeeksWorkTime.NetWorkTime = newStats.ThisWeeksWorkTime.NetWorkTime;
			this.ThisWeeksWorkTime.Update(newStats.ThisWeeksWorkTime);
			this.ThisMonthsWorkTime.NetWorkTime = newStats.ThisMonthsWorkTime.NetWorkTime;
			this.ThisMonthsWorkTime.Update(newStats.ThisMonthsWorkTime);
			UpdateTodaysWorksByWorkId(this, newStats); //we have to call UpdateTodaysWorksByWorkId before UpdateCurrentWorks in order to display the right names
			UpdateCurrentWorks(this, newStats);
			UpdateOnlineComputers(this, newStats);
			this.ThisMonthsTargetNetWorkTime = newStats.ThisMonthsTargetNetWorkTime;
			this.ThisMonthsTargetUntilTodayNetWorkTime = newStats.ThisMonthsTargetUntilTodayNetWorkTime;
			this.ThisWeeksTargetNetWorkTime = newStats.ThisWeeksTargetNetWorkTime;
			this.ThisWeeksTargetUntilTodayNetWorkTime = newStats.ThisWeeksTargetUntilTodayNetWorkTime;
			this.TodaysTargetNetWorkTime = newStats.TodaysTargetNetWorkTime;
			this.HasComputerActivity = newStats.HasComputerActivity;
			this.HasRemoteDesktop = newStats.HasRemoteDesktop;
			this.HasVirtualMachine = newStats.HasVirtualMachine;
			UpdateIPAddresses(this, newStats);
		}

		private static void UpdateIPAddresses(BriefUserStats oldObj, BriefUserStats newObj)
		{
			if (oldObj.IPAddresses.Count == newObj.IPAddresses.Count)
			{
				var areSame = true;
				for (int i = 0; i < oldObj.IPAddresses.Count; i++)
				{
					if (!oldObj.IPAddresses[i].Equals(newObj.IPAddresses[i]))
					{
						areSame = false;
						break;
					}
				}
				if (areSame) return;
			}
			oldObj.IPAddresses.Clear();
			foreach (var obj in newObj.IPAddresses)
			{
				oldObj.IPAddresses.Add(obj);
			}
		}

		private static void UpdateOnlineComputers(BriefUserStats oldObj, BriefUserStats newObj)
		{
			if (oldObj.OnlineComputers.Count == newObj.OnlineComputers.Count)
			{
				var areSame = true;
				for (int i = 0; i < oldObj.OnlineComputers.Count; i++)
				{
					if (!oldObj.OnlineComputers[i].Equals(newObj.OnlineComputers[i]))
					{
						areSame = false;
						break;
					}
				}
				if (areSame) return;
			}
			oldObj.OnlineComputers.Clear();
			foreach (var compId in newObj.OnlineComputers)
			{
				oldObj.OnlineComputers.Add(compId);
			}
		}

		private static void UpdateCurrentWorks(BriefUserStats oldObj, BriefUserStats newObj)
		{
			if (oldObj.CurrentWorks.Count == newObj.CurrentWorks.Count)
			{
				var areSame = true;
				for (int i = 0; i < oldObj.CurrentWorks.Count; i++)
				{
					if (!oldObj.CurrentWorks[i].Equals(newObj.CurrentWorks[i]))
					{
						areSame = false;
						break;
					}
				}
				if (areSame) return;
			}
			oldObj.CurrentWorks.Clear();
			foreach (var work in newObj.CurrentWorks)
			{
				oldObj.CurrentWorks.Add(work);
			}
			oldObj.CurrentWorkString = oldObj.GetCurrentWorkString();
		}

		private static void UpdateTodaysWorksByWorkId(BriefUserStats oldObj, BriefUserStats newObj)
		{
			var keysToRemove = new List<int>();
			foreach (var keyValue in oldObj.TodaysWorksByWorkId)
			{
				BriefWorkStats newStats;
				if (!newObj.TodaysWorksByWorkId.TryGetValue(keyValue.Key, out newStats))
				{
					keysToRemove.Add(keyValue.Key);
					oldObj.TodaysWorks.Remove(keyValue.Value);
				}
				else
				{
					keyValue.Value.Update(newStats);
				}
			}
			foreach (var key in keysToRemove)
			{
				oldObj.TodaysWorksByWorkId.Remove(key);
			}
			foreach (var newStatsKeyValue in newObj.TodaysWorksByWorkId.Where(n => !oldObj.TodaysWorksByWorkId.ContainsKey(n.Key)))
			{
				oldObj.TodaysWorksByWorkId.Add(newStatsKeyValue.Key, newStatsKeyValue.Value);
				oldObj.TodaysWorks.Add(newStatsKeyValue.Value);
			}
		}

		private string GetCurrentWorkString()
		{
			if (this.CurrentWorks == null || this.CurrentWorks.Count == 0) return "";
			return string.Join(", ", this.CurrentWorks.Select(n => GetWorkName(n.WorkId, this.TodaysWorksByWorkId) + " [" + n.Type + "]"));
		}

		private static string GetWorkName(int id, Dictionary<int, BriefWorkStats> works)
		{
			BriefWorkStats workStats;
			if (works != null && works.TryGetValue(id, out workStats) && workStats != null)
			{
				return workStats.WorkName + " (" + workStats.WorkId + ")";
			}
			return "Unknown work (" + id + ")";
		}
	}
}
