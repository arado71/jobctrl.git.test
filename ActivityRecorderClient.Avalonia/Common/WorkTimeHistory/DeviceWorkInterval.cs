using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Menu.Selector;
using Tct.ActivityRecorderService;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public enum DeviceType
	{
		Computer,
		Mobile,
		Ivr,
		Manual,
		Meeting,
		Holiday,
		SickLeave,
	}

	public class DeviceWorkInterval : Interval, IEquatable<DeviceWorkInterval>
	{
		public DeviceType DeviceType { get; set; }
		public int WorkId { get; set; }
		public long DeviceId { get; set; }
		public string Description { get; set; }
		public string Subject { get; set; }
		public bool IsPending { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsEditable { get; set; }
		public ManualInterval OriginalInterval { get; set; }

		public bool IsIncludedInWorkTime
		{
			get
			{
				return !IsPending && !IsDeleted;
			}
		}

		public bool IsVisible
		{
			get
			{
				switch (DeviceType)
				{
					case DeviceType.Holiday:
					case DeviceType.SickLeave:
						return false;
					default:
						return true;
				}
			}
		}

		public DeviceWorkInterval(Interval interval)
			: base(interval)
		{
			IsEditable = true;
		}

		public DeviceWorkInterval(DateTime startTime, DateTime endTime)
			: base(startTime, endTime)
		{
			IsEditable = true;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof(DeviceWorkInterval)) return false;
			return Equals((DeviceWorkInterval)obj);
		}

		public bool Equals(DeviceWorkInterval other)
		{
			return base.Equals(other) && DeviceType == other.DeviceType && WorkId == other.WorkId;
		}

		public override int GetHashCode()
		{
			return (base.GetHashCode() * 31 + ((int)DeviceType).GetHashCode()) * 31 + WorkId.GetHashCode();
		}

		public static IEnumerable<DeviceWorkInterval> GetWorks(ClientWorkTimeHistory history)
		{
			var comEditableDeletes = new IntervalConcatenator();
			var comDeletes = new IntervalConcatenator();
			var ivrEditableDeletes = new IntervalConcatenator();
			var ivrDeletes = new IntervalConcatenator();
			var mobEditableDeletes = new IntervalConcatenator();
			var mobDeletes = new IntervalConcatenator();
			var intervalAdds = new IntervalConcatenator();

			var addedIntervals = new List<ManualInterval>();
			if (history == null) yield break;

			if (history.ManualIntervals != null)
				foreach (var interval in history.ManualIntervals)
				{
					switch (interval.ManualWorkItemType)
					{
						case ManualWorkItemTypeEnum.DeleteComputerInterval:
							comDeletes.Add(interval.StartDate, interval.EndDate);
							if (interval.IsEditable) comEditableDeletes.Add(interval.StartDate, interval.EndDate);
							break;
						case ManualWorkItemTypeEnum.DeleteIvrInterval:
							ivrDeletes.Add(interval.StartDate, interval.EndDate);
							if (interval.IsEditable) ivrEditableDeletes.Add(interval.StartDate, interval.EndDate);
							break;
						case ManualWorkItemTypeEnum.DeleteMobileInterval:
							mobDeletes.Add(interval.StartDate, interval.EndDate);
							if (interval.IsEditable) mobEditableDeletes.Add(interval.StartDate, interval.EndDate);
							break;
						case ManualWorkItemTypeEnum.DeleteInterval:
							comDeletes.Add(interval.StartDate, interval.EndDate);
							ivrDeletes.Add(interval.StartDate, interval.EndDate);
							mobDeletes.Add(interval.StartDate, interval.EndDate);
							if (interval.IsEditable)
							{
								comEditableDeletes.Add(interval.StartDate, interval.EndDate);
								ivrEditableDeletes.Add(interval.StartDate, interval.EndDate);
								mobEditableDeletes.Add(interval.StartDate, interval.EndDate);
							}
							break;
						case ManualWorkItemTypeEnum.AddWork:
						case ManualWorkItemTypeEnum.AddHoliday:
						case ManualWorkItemTypeEnum.AddSickLeave:
							intervalAdds.Add(interval.StartDate, interval.EndDate);
							addedIntervals.Add(interval);
							break;
					}
				}

			if (history.ComputerIntervals != null)
			{
				var comUndoableRemoval = comEditableDeletes.Clone().Subtract(intervalAdds);
				foreach (var interval in history.ComputerIntervals)
				{
					var conc = new IntervalConcatenator();
					conc.Add(interval.StartDate, interval.EndDate);
					conc.Subtract(comDeletes);
					foreach (var calculatedInterval in conc.GetIntervals())
					{
						yield return
							new DeviceWorkInterval(calculatedInterval.StartDate, calculatedInterval.EndDate)
							{
								DeviceType = DeviceType.Computer,
								DeviceId = interval.ComputerId,
								WorkId = interval.WorkId
							};
					}

					conc = new IntervalConcatenator();
					conc.Add(interval.StartDate, interval.EndDate);
					foreach (var calculatedInterval in Intersect(conc, comUndoableRemoval).GetIntervals())
					{
						yield return new DeviceWorkInterval(calculatedInterval.StartDate, calculatedInterval.EndDate)
						{
							DeviceType = DeviceType.Computer,
							DeviceId = interval.ComputerId,
							WorkId = interval.WorkId,
							IsDeleted = true
						};
					}
				}
			}

			if (history.MobileIntervals != null)
			{
				var mobileUndoableRemoval = mobEditableDeletes.Clone().Subtract(intervalAdds);
				foreach (var interval in history.MobileIntervals)
				{
					var conc = new IntervalConcatenator();
					conc.Add(interval.StartDate, interval.EndDate);
					conc.Subtract(mobDeletes);
					foreach (var calculatedInterval in conc.GetIntervals())
					{
						yield return
							new DeviceWorkInterval(calculatedInterval.StartDate, calculatedInterval.EndDate)
							{
								DeviceType = DeviceType.Mobile,
								DeviceId = interval.Imei,
								WorkId = interval.WorkId
							};
					}

					conc = new IntervalConcatenator();
					conc.Add(interval.StartDate, interval.EndDate);
					foreach (var calculatedInterval in Intersect(conc, mobileUndoableRemoval).GetIntervals())
					{
						yield return new DeviceWorkInterval(calculatedInterval.StartDate, calculatedInterval.EndDate)
						{
							DeviceType = DeviceType.Mobile,
							DeviceId = interval.Imei,
							WorkId = interval.WorkId,
							IsDeleted = true
						};
					}
				}
			}

			foreach (var interval in addedIntervals)
			{
				yield return new DeviceWorkInterval(interval.StartDate, interval.EndDate)
				{
					DeviceType = GetDeviceType(interval),
					WorkId = interval.WorkId,
					Subject = interval.Subject,
					Description = interval.IsMeeting ? interval.Description : interval.Comment,
					OriginalInterval = interval,
					IsPending = interval.IsPending,
					IsEditable = interval.IsEditable,
				};
			}
		}

		private static IntervalConcatenator Intersect(IntervalConcatenator a, IntervalConcatenator b)
		{
			var res = a.Clone();
			var a1 = a.Clone();
			var b2 = b.Clone();
			a1.Subtract(b);
			b2.Subtract(a);
			res = res.Merge(b);
			res = res.Subtract(a1);
			res = res.Subtract(b2);
			return res;
		}

		private static DeviceType GetDeviceType(ManualInterval interval)
		{
			switch (interval.ManualWorkItemType)
			{
				case ManualWorkItemTypeEnum.AddWork:
					return interval.IsMeeting ? DeviceType.Meeting : DeviceType.Manual;
				case ManualWorkItemTypeEnum.AddHoliday:
					return DeviceType.Holiday;
				case ManualWorkItemTypeEnum.AddSickLeave:
					return DeviceType.SickLeave;
				default:
					Debug.Fail("Unknown manual work item type");
					return DeviceType.Manual;
			}
		}
	}
}
