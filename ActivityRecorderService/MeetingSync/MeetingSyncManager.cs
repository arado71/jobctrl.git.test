using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderService.Meeting;
using Tct.ActivityRecorderService.WebsiteServiceReference;

namespace Tct.ActivityRecorderService.MeetingSync
{
	public class MeetingSyncManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IGoogleCalendarSource googleCalendarSource;
		private const int CntMeetingsToSend = 300;

		public MeetingSyncManager(IGoogleCalendarSource calendarSource) : base(log)
		{
			googleCalendarSource = calendarSource;
#if DEBUG
			ManagerCallbackInterval = 60 * 1000;
#else
			ManagerCallbackInterval = ConfigManager.MeetingSyncInterval;
#endif
		}

#if DEBUG
		public DateTime? LastSuccessfulSyncDebug { get; set; }
#endif

		protected override void ManagerCallbackImpl()
		{
			PerformSync(DateTime.UtcNow);
		}

		// return value for testing purposes
		public List<List<FinishedMeetingEntry>> PerformSync(DateTime syncTime)
		{
			using (var context1 = new ActivityRecorderDataClassesDataContext())
			{
				var cnts = new List<List<FinishedMeetingEntry>>();
				foreach (var cloudToken in context1.Client_GetCloudTokens(syncTime.AddMilliseconds(-ManagerCallbackInterval)))
				{
					if (!UserIdManager.Instance.IsActive(cloudToken.UserId)) continue;
					var mtgcnt = 0;
					var sw = Stopwatch.StartNew();
					List<FinishedMeetingEntry> meetings = null;
					using (var context = new ActivityRecorderDataClassesDataContext())
					{
						context.Connection.Open();
						context.Connection.SetXactAbortOn();
						using (context.Transaction = context.Connection.BeginTransaction())
						{
							try
							{
								var syncToken = cloudToken.SyncToken;
								var lastUpdateTime = cloudToken.LastUpdateTime;
								var eventsAfter = syncTime.AddDays(-ConfigManager.MaxWorkItemAgeInDays);
#if !DEBUG
								using (var jccontext = new JobControlDataClassesDataContext())
								using (var client = new Website.WebsiteClientWrapper())
								{
									var mmr = new ManageMeetingsRequest { LoginTicket = new Guid(jccontext.GetAuthTicket(cloudToken.UserId)), ComputerId = 1, MeetingsToUpload = null, LastQueryIntervalEndDate = null };
									var data = client.Client.ManageMeetings(mmr);
#else
								{
									var data = new ManageMeetingsResponse() { LastSuccessfulSyncDate = LastSuccessfulSyncDebug };
#endif
									log.Debug($@"Api.ManageMeetings with LastQueryIntervalEndDate=null userId: {cloudToken.UserId} " + $@"results LastSuccessfulSyncDate: {data.LastSuccessfulSyncDate}," + $@" WebSiteResultCode: {data.Result} lastUpdateTime: {lastUpdateTime}");
									if (data.LastSuccessfulSyncDate == null || lastUpdateTime == null || data.LastSuccessfulSyncDate?.ToString() != lastUpdateTime.ToString())
									{
										syncToken = null;
										if (data.LastSuccessfulSyncDate != null && lastUpdateTime.HasValue && data.LastSuccessfulSyncDate.Value < lastUpdateTime.Value)
										{
											context.ClientUserCloudEventDates.DeleteAllOnSubmit(context.ClientUserCloudEventDates.Where(d => d.UserId == cloudToken.UserId && d.StartTime >= data.LastSuccessfulSyncDate.Value));
											context.SubmitChanges();
											eventsAfter = data.LastSuccessfulSyncDate.Value;
										}
										lastUpdateTime = data.LastSuccessfulSyncDate;
									}

									meetings = googleCalendarSource.GetEvents(cloudToken.UserId, ref syncToken, eventsAfter, cloudToken.IsMeetingTentativeSynced ?? true)?.Distinct(new FinishedMeetingEntryComparer())?.ToList();
									if (meetings != null)
									{
										foreach (var meeting in meetings)
										{
											var hash = meeting.Id.GetHashCode();
											var item = context.ClientUserCloudEventDates.SingleOrDefault(ed => ed.UserId == cloudToken.UserId && ed.EventIdHash == hash && ed.EventId == meeting.Id);
											if (item != null)
											{
												meeting.OldStartTime = item.StartTime;
												if (meeting.Status.HasValue && meeting.Status.Value == MeetingCrudStatus.Deleted)
												{
													context.ClientUserCloudEventDates.DeleteOnSubmit(item);
												}
												else
												{
													item.StartTime = meeting.StartTime;
													meeting.Status = MeetingCrudStatus.Updated;
												}
											}
											else if (!meeting.Status.HasValue || meeting.Status.Value != MeetingCrudStatus.Deleted)
											{
												item = new ClientUserCloudEventDate { UserId = cloudToken.UserId, EventId = meeting.Id, EventIdHash = meeting.Id.GetHashCode(), StartTime = meeting.StartTime, };
												context.ClientUserCloudEventDates.InsertOnSubmit(item);
												meeting.Status = MeetingCrudStatus.Created;
											}
										}
										context.SubmitChanges();
									}
									else
									{
										log.Warn("Unsuccessful sync, clearing lastUpdateTime.");
										lastUpdateTime = null;
									}
									while (meetings != null && mtgcnt < meetings.Count)
									{
										var meetingsToSend = meetings.Skip(mtgcnt).Take(CntMeetingsToSend).ToList();
										lastUpdateTime = DateTime.UtcNow;
#if !DEBUG
										ManageMeetingsRequest req = new ManageMeetingsRequest() { LoginTicket = new Guid(jccontext.GetAuthTicket(cloudToken.UserId)), ComputerId = 1, MeetingsToUpload = MeetingDataMapper.To(meetingsToSend), LastQueryIntervalEndDate = lastUpdateTime, };
										//log.DebugFormat("Api.ManageMeetings calling with userId:{0}, ComputerId:{1}, MessageCnt:{2} LastQueryIntervalEndDate:{3}", cloudToken.UserId, req.ComputerId, req.MeetingsToUpload.Length, req.LastQueryIntervalEndDate);
										var res = client.Client.ManageMeetings(req);
#else
										LastSuccessfulSyncDebug = lastUpdateTime;
										var res = new ManageMeetingsResponse() { LastSuccessfulSyncDate = LastSuccessfulSyncDebug, Result = ManageMeetingsRet.OK };
#endif
										log.Debug($@"Api.ManageMeetings userId: {cloudToken.UserId} results LastSuccessfulSyncDate: {res.LastSuccessfulSyncDate}, WebSiteResultCode: {res.Result}");
										var msgDbg =  string.Join("\n", meetingsToSend.OrderBy(m => m.StartTime).Take(10).Select(m => $"Subject:{m.Title} [{m.StartTime}-{m.EndTime}] C:{m.CreationTime} M:{m.LastmodificationTime} Old:{m.OldStartTime} S:{m.Status} Id:{m.Id}").ToArray());
										log.Debug($"UserId:{cloudToken.UserId} meetings:{meetingsToSend.Count}\n" + msgDbg);
										if (res.Result != ManageMeetingsRet.OK)
										{
											throw new FaultException($"Meetings can't be uploaded to Api.ManageMeetings userId: {cloudToken.UserId}, WebSiteResultCode: {res.Result}");
										}
										mtgcnt += meetingsToSend.Count;
									}
								}
								context.Client_UpdateCloudToken(cloudToken.UserId, syncToken, 0, lastUpdateTime, syncTime);
								context.Transaction.Commit();
							}
							catch (Exception ex)
							{
								context.Transaction.Rollback();
								log.Error($@"MeetingSync UserId: {cloudToken.UserId} failed", ex);
							}
							finally
							{
								log.Debug($@"MeetingSync UserId: {cloudToken.UserId} cnt: {mtgcnt} finished in {sw.ElapsedMilliseconds} ms");
								cnts.Add(meetings);
							}
						}
					}
				}
				log.Info($@"MeetingSync processed {cnts.Count} calendars");
				return cnts;
			}
		}

		private class FinishedMeetingEntryComparer : IEqualityComparer<FinishedMeetingEntry>
		{
			public bool Equals(FinishedMeetingEntry x, FinishedMeetingEntry y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null)) return false;
				if (ReferenceEquals(y, null)) return false;
				if (x.GetType() != y.GetType()) return false;
				return x.Id == y.Id && x.CreationTime.Equals(y.CreationTime) && x.LastmodificationTime.Equals(y.LastmodificationTime) && x.Title == y.Title && x.StartTime.Equals(y.StartTime) && x.EndTime.Equals(y.EndTime) && Nullable.Equals(x.OldStartTime, y.OldStartTime) && x.Status == y.Status;
			}

			public int GetHashCode(FinishedMeetingEntry obj)
			{
				unchecked
				{
					var hashCode = (obj.Id != null ? obj.Id.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ obj.CreationTime.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.LastmodificationTime.GetHashCode();
					hashCode = (hashCode * 397) ^ (obj.Title != null ? obj.Title.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ obj.StartTime.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.EndTime.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.OldStartTime.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.Status.GetHashCode();
					return hashCode;
				}
			}
		}
	}
}
