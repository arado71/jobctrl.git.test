using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.WorkTimeHistory
{
	/// <summary>
	/// Class for applying worktime modifications via website's api.
	/// </summary>
	/// <remarks>
	/// Website supports no transactions, so the error handling is less than ideal...
	/// If we have an error during the first modification then we will exit.
	/// Otherwise we will continue with the next modification even if the previous have failed.
	/// </remarks>
	public static class ModifyWorkTimeHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal static readonly IComparer<ManualIntervalModification> comparer = new WorkTimeModificationsComparer();

		// ReSharper disable AccessToDisposedClosure, AccessToForEachVariableInClosure
		public static bool ModifyWorkTime(int userId, int gorupId, int companyId, int createdBy, WorkTimeModifications modifications)
		{
			log.Info("Executing modifications for uid: " + userId.ToInvariantString() + " by: " + createdBy.ToInvariantString() + " " + modifications);
			ValidateModifications(userId, modifications);
			var result = true;
			var isFirst = true;
			using (var client = new Website.WebsiteClientWrapper())
			using (var context = new JobControlDataClassesDataContext())
			{
				foreach (var modification in modifications.ManualIntervalModifications.OrderBy(n => n, comparer))
				{
					log.Debug("Executing modification for uid: " + userId.ToInvariantString() + " mod: " + modification);
					if (modification.OriginalItem == null && modification.NewItem == null) continue;
					if (modification.OriginalItem == null) //Insert
					{
						result &= TryExecuteWebRequest(client, n => WebAddManualWorkItem(n, new Guid(context.GetAuthTicket(userId)), modification.NewItem, modifications.ComputerId), "WebAddManualWorkItem I " + modification.NewItem, ref isFirst);
					}
					else if (modification.NewItem == null) //Delete
					{
						if (modification.OriginalItem.PendingId.HasValue) // delete pending
						{
							result &= TryExecuteWebRequest(client, n => WebRevokePending(n, new Guid(context.GetAuthTicket(userId)), modification.OriginalItem), "WebRevokePending D " + modification.OriginalItem, ref isFirst);
						}
						else if (modification.OriginalItem.MeetingId.HasValue) //delete meeting
						{
							result &= TryExecuteWebRequest(client, n => WebDeleteMeeting(n, new Guid(context.GetAuthTicket(userId)), modification.OriginalItem), "WebDeleteMeeting D " + modification.OriginalItem, ref isFirst);
						}
						else //delete manual
						{
							result &= TryExecuteWebRequest(client, n => WebDeleteManualWorkItem(n, new Guid(context.GetAuthTicket(userId)), modification.OriginalItem), "WebDeleteManualWorkItem D " + modification.OriginalItem, ref isFirst);
						}
					}
					else //Update
					{
						if (modification.OriginalItem.PendingId.HasValue) // update pending
						{
							result &= TryExecuteWebRequest(client, n => WebRevokePending(n, new Guid(context.GetAuthTicket(userId)), modification.OriginalItem), "WebRevokePending U " + modification.OriginalItem, ref isFirst);
							result &= TryExecuteWebRequest(client, n => WebAddManualWorkItem(n, new Guid(context.GetAuthTicket(userId)), modification.NewItem, modifications.ComputerId), "WebAddManualWorkItem U " + modification.NewItem, ref isFirst);
						}
						else if (modification.OriginalItem.MeetingId.HasValue) //update meeting
						{
							//Update is not supported by the web team anymore...
							//modification.NewItem.MeetingId = modification.OriginalItem.MeetingId; //NewItem's ids are not set by the client
							//result &= TryExecuteWebRequest(client, n => WebUpdateMeeting(n, new Guid(context.GetAuthTicket(userId)), modification.NewItem), "WebUpdateMeeting U " + modification.NewItem, ref isFirst);
							result &= TryExecuteWebRequest(client, n => WebDeleteMeeting(client, new Guid(context.GetAuthTicket(userId)), modification.OriginalItem), "WebDeleteMeeting U " + modification.OriginalItem, ref isFirst);
							result &= TryExecuteWebRequest(client, n => WebAddManualWorkItem(client, new Guid(context.GetAuthTicket(userId)), modification.NewItem, modifications.ComputerId), "WebAddManualWorkItem U " + modification.NewItem, ref isFirst);
						}
						else //update manual
						{
							result &= TryExecuteWebRequest(client, n => WebDeleteManualWorkItem(n, new Guid(context.GetAuthTicket(userId)), modification.OriginalItem), "WebDeleteManualWorkItem U " + modification.OriginalItem, ref isFirst);
							result &= TryExecuteWebRequest(client, n => WebAddManualWorkItem(n, new Guid(context.GetAuthTicket(userId)), modification.NewItem, modifications.ComputerId), "WebAddManualWorkItem U " + modification.NewItem, ref isFirst);
						}
					}
				}
			}
			return result;
		}
		// ReSharper restore AccessToDisposedClosure, AccessToForEachVariableInClosure

		//makes sure the ManualIntervals are editable by the user
		private static void ValidateModifications(int userId, WorkTimeModifications modifications)
		{
			if (modifications.ManualIntervalModifications.Count > 20)
			{
				log.Error("Request contains too many " + modifications.ManualIntervalModifications.Count.ToInvariantString() + " modifications uid: " + userId.ToInvariantString() + " " + modifications);
				throw new FaultException("Too many modifications");
			}
			var now = DateTime.UtcNow;
			var all = modifications.ManualIntervalModifications.SelectMany(n => new[] { n.NewItem, n.OriginalItem }).Where(n => n != null);
			foreach (var item in all)
			{
				if (item.ManualWorkItemType != ManualWorkItemTypeEnum.AddWork
					&& item.ManualWorkItemType != ManualWorkItemTypeEnum.DeleteInterval
					&& item.ManualWorkItemType != ManualWorkItemTypeEnum.DeleteComputerInterval
					&& item.ManualWorkItemType != ManualWorkItemTypeEnum.DeleteIvrInterval
					&& item.ManualWorkItemType != ManualWorkItemTypeEnum.DeleteMobileInterval
					)
				{
					log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with invalid type " + item.ManualWorkItemType + " found");
					throw new FaultException("ManualInterval " + item + " with invalid type found");
				}
				if (item.EndDate < item.StartDate)
				{
					log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with invalid interval " + item.EndDate.ToInvariantString() + " - " + item.StartDate.ToInvariantString() + " found");
					throw new FaultException("ManualInterval " + item + " with invalid interval found");
				}
			}
			var news = modifications.ManualIntervalModifications.Select(n => n.NewItem).Where(n => n != null);
			foreach (var item in news)
			{
				if (item.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork && item.EndDate > now)
				{
					log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with future end date " + item.EndDate.ToInvariantString() + " found");
					throw new FaultException("ManualInterval " + item + " with future end date found");
				}
				if (item.EndDate - item.StartDate > TimeSpan.FromHours(24))
				{
					log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with too long interval " + (item.EndDate - item.StartDate).ToHourMinuteString() + " found");
					throw new FaultException("ManualInterval " + item + " with too long interval found");
				}
			}

			var origs = modifications.ManualIntervalModifications.Select(n => n.OriginalItem).Where(n => n != null && !n.PendingId.HasValue); //we don't validate pendings here
			using (var context = new ManualDataClassesDataContext())
			{
				foreach (var item in origs)
				{
					var dbItem = context.ManualWorkItems.SingleOrDefault(n => n.Id == item.Id && n.UserId == userId);
					if (dbItem == null)
					{
						log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with invalid Id " + item.Id.ToInvariantString() + " found");
						throw new FaultException("ManualInterval " + item + " with invalid Id found");
					}
					if (dbItem.ManualWorkItemTypeId != item.ManualWorkItemType)
					{
						log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with invalid ManualWorkItemType (" + dbItem.ManualWorkItemTypeId + "!=" + item.ManualWorkItemType + ") found");
						throw new FaultException("ManualInterval " + item + " with invalid SourceId found");
					}
					if (dbItem.SourceId != item.SourceId)
					{
						log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with invalid SourceId (" + dbItem.SourceId + "!=" + item.SourceId + ") found");
						throw new FaultException("ManualInterval " + item + " with invalid SourceId found");
					}
					if (!item.IsEditable) //IsEditable only depends on SourceId for non-pendings atm.
					{
						log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " is not editable");
						throw new FaultException("ManualInterval " + item + " is not editable");
					}
				}
			}

			var pendings = modifications.ManualIntervalModifications.Select(n => n.OriginalItem).Where(n => n != null && n.PendingId.HasValue).ToArray(); //pendings are validated here
			if (pendings.Length > 0)
			{
				var minStartDate = pendings.Select(n => n.StartDate).Min();
				var maxEndDate = pendings.Select(n => n.EndDate).Max();
				var histDict = WorkTimeHistoryDbHelper.GetWorkTimeHistory(userId, minStartDate, maxEndDate)
					.ManualIntervals
					.Where(n => n.PendingId.HasValue)
					.ToDictionary(n => n.PendingId.Value); //this is a bit heavy-weight for pendings only
				foreach (var item in pendings)
				{
					if (!histDict.ContainsKey(item.PendingId.Value))
					{
						log.Error("ManualInterval " + item + " uid: " + userId.ToInvariantString() + " with invalid PendingId " + item.PendingId.ToInvariantString() + " found");
						throw new FaultException("ManualInterval " + item + " with invalid PendingId found");
					}
					if (!item.IsEditable) //IsEditable is always true for pendings atm.
					{
						log.Error("ManualInterval " + item + " (pending) uid: " + userId.ToInvariantString() + " is not editable");
						throw new FaultException("ManualInterval " + item + " (pending) is not editable");
					}
				}
			}
		}

		private static bool TryExecuteWebRequest(Website.WebsiteClientWrapper client, Func<Website.WebsiteClientWrapper, string> webRequest, string name, ref bool isFirst)
		{
			const int maxTries = 3;
			var tries = maxTries;
			string lastError = null;
			while (tries-- > 0)
			{
				var sw = Stopwatch.StartNew();
				try
				{
					lastError = webRequest(client);
					if (lastError == null)
					{
						log.Debug("Successfully executed (" + (maxTries - tries).ToInvariantString() + ") " + name + " in " + sw.ToTotalMillisecondsString() + "ms");
						isFirst = false;
						return true;
					}
					log.Error("Failed to execute (" + (maxTries - tries).ToInvariantString() + ") " + name + " result: " + lastError + " in " + sw.ToTotalMillisecondsString() + "ms");
				}
				catch (Exception ex)
				{
					lastError = ex.Message;
					log.Error("Error while executing (" + (maxTries - tries).ToInvariantString() + ") " + name + " in " + sw.ToTotalMillisecondsString() + "ms", ex);
					client.RefreshClient();
				}
			}
			if (isFirst) throw new FaultException("Cannot execute the first modification " + name + " error: " + lastError); //don't continue further
			return false;
		}

		private static string WebAddManualWorkItem(Website.WebsiteClientWrapper client, Guid ticket, ManualInterval item, int? computerId)
		{
			return client.Client.AddManualWorkItem(
							ticket,
							MapTypeFrom(item.ManualWorkItemType),
							item.StartDate,
							item.EndDate,
							item.GetWorkId(),
							item.Comment,
							item.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval ? computerId : null,
							item.SourceId == (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting? WebsiteServiceReference.ManualWorkItemSource.ServerAddWorkItem : (WebsiteServiceReference.ManualWorkItemSource?)null);
		}

		private static string WebRevokePending(Website.WebsiteClientWrapper client, Guid ticket, ManualInterval item)
		{
			return client.Client.SetRequestedManualWorkItemStatus(
							ticket,
							item.PendingId.Value,
							WebsiteServiceReference.RequestedManualWorkItemStatus.Revoked);
		}

		private static string WebDeleteMeeting(Website.WebsiteClientWrapper client, Guid ticket, ManualInterval item)
		{
			return client.Client.DeleteMeeting(ticket, item.MeetingId.Value);
		}

		private static string WebDeleteManualWorkItem(Website.WebsiteClientWrapper client, Guid ticket, ManualInterval item)
		{
			return client.Client.DeleteManualWorkItem(ticket, item.Id);
		}

		private static string WebUpdateMeeting(Website.WebsiteClientWrapper client, Guid ticket, ManualInterval item)
		{
			return client.Client.UpdateMeeting(ticket, item.MeetingId.Value, item.WorkId, item.StartDate, item.EndDate);
		}

		private static WebsiteServiceReference.ManualWorkItemType MapTypeFrom(ManualWorkItemTypeEnum type)
		{
			switch (type)
			{
				case ManualWorkItemTypeEnum.AddWork:
					return WebsiteServiceReference.ManualWorkItemType.AddWork;
				case ManualWorkItemTypeEnum.DeleteInterval:
					return WebsiteServiceReference.ManualWorkItemType.DeleteInterval;
				case ManualWorkItemTypeEnum.DeleteIvrInterval:
					return WebsiteServiceReference.ManualWorkItemType.DeleteIvrInterval;
				case ManualWorkItemTypeEnum.DeleteComputerInterval:
					return WebsiteServiceReference.ManualWorkItemType.DeleteComputerInterval;
				case ManualWorkItemTypeEnum.DeleteMobileInterval:
					return WebsiteServiceReference.ManualWorkItemType.DeleteMobileInterval;
				default:
					throw new ArgumentOutOfRangeException("type"); //don't allow sickleaves and holidays
			}
		}

		/// <summary>
		/// Class for trying to order modifications in a way to avoid parallel worktime validation errors. Not water-tight at all...
		/// </summary>
		private class WorkTimeModificationsComparer : IComparer<ManualIntervalModification>
		{
			public int Compare(ManualIntervalModification x, ManualIntervalModification y)
			{
				if (x == null) return y == null ? 0 : -1;
				if (y == null) return 1;

				return GetBase(x) - GetBase(y);
			}

			private static int GetBase(ManualIntervalModification x)
			{
				Debug.Assert(x != null);
				if (x == null) return 0;
				//The order of operations:
				//Insert Deletes
				//Delete Adds
				//Update Deletes (lengthen)

				//Update Deletes (move)    //after this we might have an inconsistent state
				//Update Adds (move)

				//Update Adds (shorten)
				//Update Deletes (shorten)
				//Update Adds (lengthen)
				//Insert Adds
				//Delete Deletes

				var mod = GetModificationType(x);
				var isAdd = IsAddWork(x);

				if (mod == ModificationType.Insert && !isAdd) return 1;
				if (mod == ModificationType.Delete && isAdd) return 2;
				if (mod == ModificationType.UpdateLengthen && !isAdd) return 3;
				if (mod == ModificationType.UpdateMove) return !isAdd ? 4 : 5;
				if (mod == ModificationType.UpdateShorten) return isAdd ? 6 : 7;
				if (mod == ModificationType.UpdateLengthen && isAdd) return 8;
				if (mod == ModificationType.Insert && isAdd) return 9;
				if (mod == ModificationType.Delete && !isAdd) return 10;

				log.ErrorAndFail("Cannot get base for ManualIntervalModification " + x);
				return 11;
			}

			private static bool IsAddWork(ManualIntervalModification x)
			{
				switch (GetModificationType(x))
				{
					case ModificationType.Insert:
						return x.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork;
					case ModificationType.Delete:
						return x.OriginalItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork;
					case ModificationType.UpdateLengthen:
					case ModificationType.UpdateShorten:
					case ModificationType.UpdateMove:
						Debug.Assert(x.OriginalItem.ManualWorkItemType == x.NewItem.ManualWorkItemType);
						return x.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork;
					default:
						return false;
				}
			}

			private static ModificationType GetModificationType(ManualIntervalModification x)
			{
				if (x == null || x.OriginalItem == null && x.NewItem == null) return ModificationType.None;
				if (x.OriginalItem == null) return ModificationType.Insert;
				if (x.NewItem == null) return ModificationType.Delete;
				if (x.OriginalItem.StartDate <= x.NewItem.StartDate && x.OriginalItem.EndDate >= x.NewItem.EndDate) return ModificationType.UpdateShorten;
				if (x.OriginalItem.StartDate >= x.NewItem.StartDate && x.OriginalItem.EndDate <= x.NewItem.EndDate) return ModificationType.UpdateLengthen;
				return ModificationType.UpdateMove;
			}

			private enum ModificationType
			{
				None = 0,
				Insert,
				Delete,
				UpdateLengthen,
				UpdateShorten,  //use this for unchanged intervals also
				UpdateMove,
			}
		}
	}
}
