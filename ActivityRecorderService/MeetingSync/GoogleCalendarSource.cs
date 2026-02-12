using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using log4net;

namespace Tct.ActivityRecorderService.MeetingSync
{
	public class GoogleCalendarSource : IGoogleCalendarSource
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IDataStore dataStore;
		private readonly Regex timeZoneCut = new Regex(@"([+-]\d\d\:\d\d)|Z$");

		private static readonly ClientSecrets clientSecrets = new ClientSecrets()
		{
			ClientId = ConfigManager.GoogleClientId,
			ClientSecret = ConfigManager.GoogleClientSecret
		};

		private const string applicationName = "JobCTRL";

		public GoogleCalendarSource(IDataStore dataStore)
		{
			this.dataStore = dataStore;
		}

		private CalendarService GetCalendarService(int userId)
		{
			var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
			{
				ClientSecrets = clientSecrets,
				DataStore = dataStore,
			});
			var cred = flow.LoadTokenAsync(userId.ToString(), CancellationToken.None).Result;
			var calserv = new CalendarService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = new UserCredential(flow, userId.ToString(), cred),
				ApplicationName = applicationName,
			});
			return calserv;
		}

		public List<FinishedMeetingEntry> GetEvents(int userId, ref string syncToken, DateTime eventsAfter, bool needTentativeMeetings)
		{
			var events = new List<FinishedMeetingEntry>();
			var req = GetCalendarService(userId).Events.List("primary");
			if (string.IsNullOrEmpty(syncToken))
			{
				req.TimeMin = eventsAfter;
			}
			else
				req.SyncToken = syncToken;
			req.SingleEvents = true;
			req.ShowDeleted = true;
			string pageToken = null;
			Events evs;
			do
			{
				req.PageToken = pageToken;
				try
				{
					evs = req.Execute();
				}
				catch (GoogleApiException ex)
				{
					switch (ex.Error.Code)
					{
						case 403:
							log.Warn($@"AccessToken doesn't have enough rights, can't be refresh ({ex.Message})");
							dataStore.DeleteAsync<object>(userId.ToString());
							return null;
						case 410:
							log.Warn($@"SyncToken is invalid, retrying with full sync... ({ex.Message})");
							syncToken = null;
							return null;
						default:
							throw;
					}
				}
				catch (TokenResponseException ex)
				{
					log.Warn($@"AccessToken is invalid, needs to refresh ({ex.Message})");
					return null;
				}
				catch (InvalidOperationException ex)
				{
					log.Warn($@"AccessToken is invalid, can't be refresh ({ex.Message})");
					dataStore.DeleteAsync<object>(userId.ToString());
					return null;
				}
				var items = evs.Items;
				foreach (var ev in items)
				{
					if (!needTentativeMeetings && ev.Status == "tentative" || ev.Attendees == null || ev.Attendees.All(a => a.Organizer ?? false)) continue;
					events.Add(new FinishedMeetingEntry
					{
						Title = ev.Summary,
						Description = ev.Description,
						Attendees = ev.Attendees?.Select(a => new MeetingAttendee{Email = a.Email, ResponseStatus = Map(a.ResponseStatus), Type = a.Organizer.HasValue && a.Organizer.Value ? MeetingAttendeeType.Organizer : MeetingAttendeeType.Required}).ToList(),
						CreationTime = ev.Created ?? default(DateTime),
						StartTime = ev.Start != null ? Convert(ev.Start) : default(DateTime),
						EndTime = ev.End != null ? Convert(ev.End) : default(DateTime),
						Location = ev.Location,
						Id = ev.Id,
						IsInFuture = ev.Start != null && Convert(ev.Start) >= DateTime.UtcNow,
						LastmodificationTime = ev.Updated ?? DateTime.MinValue,
						Status = ev.Status == "cancelled" ? MeetingCrudStatus.Deleted : (MeetingCrudStatus?)null,
					});
				}
				pageToken = evs.NextPageToken;
			} while (pageToken != null);
			syncToken = evs.NextSyncToken;
			return events;
		}

		private DateTime Convert(EventDateTime evtim)
		{ // 2018-12-12T13:00:00+01:00 to UTC!!!
		  // 2022-01-11T09:00:00Z is already in UTC
			if (!string.IsNullOrEmpty(evtim.Date)) return DateTime.ParseExact(evtim.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
			var offsetString = timeZoneCut.Match(evtim.DateTimeRaw).Value;
			var offsetTimeSpan = offsetString == "Z" ? TimeSpan.Zero : TimeSpan.ParseExact(offsetString.Substring(1), "hh\\:mm", null, offsetString[0] == '-' ? TimeSpanStyles.AssumeNegative : TimeSpanStyles.None);
			return evtim.DateTime - offsetTimeSpan ?? DateTime.MinValue;
		}

		private static MeetingAttendeeResponseStatus Map(string status)
		{
			switch (status)
			{
				case "accepted":
					return MeetingAttendeeResponseStatus.ResponseAccepted;
				case "declined":
					return MeetingAttendeeResponseStatus.ResponseDeclined;
				case "tentative":
					return MeetingAttendeeResponseStatus.ResponseTentative;
				case "needsAction":
					return MeetingAttendeeResponseStatus.ResponseNotResponded;
				default:
					return MeetingAttendeeResponseStatus.ResponseNone;
			}
		}

	}
}
