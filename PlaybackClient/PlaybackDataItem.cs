using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PlaybackClient.ActivityRecorderServiceReference;
using MobileRequest = PlaybackClient.MobileServiceReference.UploadData_v4Request;

namespace PlaybackClient
{
	//immutable class (thread-safe)
	public class PlaybackDataItem : IComparable<PlaybackDataItem>
	{
		public readonly WorkItem WorkItem;
		public readonly ManualWorkItem ManualWorkItem;
		public readonly MobileRequest MobileRequest;
		public readonly int RetryCount;

		private PlaybackDataItem(WorkItem workItem, ManualWorkItem manualWorkItem, MobileRequest mobileRequest, int retryCount = 0, DateTime? scheduledTime = null)
		{
			WorkItem = workItem;
			ManualWorkItem = manualWorkItem;
			MobileRequest = mobileRequest;
			RetryCount = retryCount;
			this.scheduledTime = scheduledTime;
		}

		public PlaybackDataItem(WorkItem workItem)
			: this(workItem, null, null)
		{
		}

		public PlaybackDataItem(ManualWorkItem workItem)
			: this(null, workItem, null)
		{
		}

		public PlaybackDataItem(MobileRequest mobileRequest)
			: this(null, null, mobileRequest)
		{
		}

		public PlaybackDataItem(PlaybackDataItem source)
		{
			if (source.WorkItem != null) WorkItem = new WorkItem
			{
				UserId = source.WorkItem.UserId,
				ComputerId = source.WorkItem.ComputerId,
				DesktopCaptures = source.WorkItem.DesktopCaptures.ToList(),
				EndDate = source.WorkItem.EndDate,
				ExtensionData = source.WorkItem.ExtensionData,
				IsRemoteDesktop = source.WorkItem.IsRemoteDesktop,
				IsVirtualMachine = source.WorkItem.IsVirtualMachine,
				KeyboardActivity = source.WorkItem.KeyboardActivity,
				MouseActivity = source.WorkItem.MouseActivity,
				PhaseId = Guid.NewGuid(),
				StartDate = source.WorkItem.StartDate,
				WorkId = source.WorkItem.WorkId,
			};
			if (source.ManualWorkItem != null) ManualWorkItem = new ManualWorkItem
			{
				UserId = source.ManualWorkItem.UserId,
				StartDate = source.ManualWorkItem.StartDate,
				EndDate = source.ManualWorkItem.EndDate,
				WorkId = source.ManualWorkItem.WorkId,
				ExtensionData = source.ManualWorkItem.ExtensionData,
				Comment = source.ManualWorkItem.Comment,
				ManualWorkItemTypeId = source.ManualWorkItem.ManualWorkItemTypeId,
				OriginalEndDate = source.ManualWorkItem.OriginalEndDate,
			};
			if (source.MobileRequest != null) MobileRequest = new MobileRequest
			{
				UserId = source.MobileRequest.UserId,
				WorkId = source.MobileRequest.WorkId,
				Errors = source.MobileRequest.Errors.ToList(),
				Imei = source.MobileRequest.Imei,
				KickId = source.MobileRequest.KickId,
				KickStatus = source.MobileRequest.KickStatus,
				LocationInfos = source.MobileRequest.LocationInfos.ToList(),
				Password = source.MobileRequest.Password,
				PhoneCalls = source.MobileRequest.PhoneCalls.ToList(),
				PhoneNumberVersion = source.MobileRequest.PhoneNumberVersion,
				ReasonResponses = source.MobileRequest.ReasonResponses.ToList(),
				RuleVersion = source.MobileRequest.RuleVersion,
				Rules = source.MobileRequest.Rules.ToList(),
				Status = source.MobileRequest.Status,
				TaskAssignments = source.MobileRequest.TaskAssignments.ToList(),
				WorkItems = source.MobileRequest.WorkItems.ToList(),
				WorkPhoneNumbers = source.MobileRequest.WorkPhoneNumbers.ToList(),
			};
			RetryCount = source.RetryCount;
			scheduledTime = source.scheduledTime;
		}

		private readonly DateTime? scheduledTime;
		public DateTime ScheduledTime
		{
			get
			{
				return scheduledTime ??
					(WorkItem != null
						? WorkItem.EndDate
						: ManualWorkItem != null
							? ManualWorkItem.EndDate
							: MobileRequest.ScheduledTime);
			}
		}

		public PlaybackDataItem GetRetryData(TimeSpan waitTime, int maxRetries)
		{
			return RetryCount < maxRetries
				? new PlaybackDataItem(WorkItem, ManualWorkItem, MobileRequest, RetryCount + 1, DateTimeEx.UtcNow().Add(waitTime))
				: null;
		}

		public void PrepareForSend()
		{
			if (WorkItem != null)
			{
				WorkItem.TryLoadScreenShots();
			}
		}

		public int CompareTo(PlaybackDataItem other)
		{
			return ScheduledTimeComparer.Compare(this, other);
		}

		public override string ToString()
		{
			return WorkItem != null
				? WorkItem.ToString()
				: ManualWorkItem != null
					? ManualWorkItem.ToString()
					: MobileRequest != null
						? MobileRequest.ToString()
						: "Unknown";
		}

		public static readonly IComparer<PlaybackDataItem> ScheduledTimeComparer = new AsyncDataScheduledTimeComparer();
		private class AsyncDataScheduledTimeComparer : IComparer<PlaybackDataItem>
		{
			public int Compare(PlaybackDataItem x, PlaybackDataItem y)
			{
				Debug.Assert(x != null);
				Debug.Assert(y != null);
				if (x == null)
				{
					return y == null ? 0 : -1;
				}
				if (y == null) return 1;

				return DateTime.Compare(x.ScheduledTime, y.ScheduledTime);
			}
		}
	}
}
