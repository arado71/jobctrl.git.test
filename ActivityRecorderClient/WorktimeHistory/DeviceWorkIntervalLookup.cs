using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderService;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public class DeviceWorkIntervalLookup
	{
		public Dictionary<DeviceType, Dictionary<long, List<DeviceWorkInterval>>> WorksByDevice
		{
			get;
			private set;
		}

		public Dictionary<int, Dictionary<DeviceType, TimeSpan>> WorkTimeById
		{
			get;
			private set;
		}

		public IEnumerable<int> WorkIds
		{
			get
			{
				return WorkTimeById.Keys;
			}
		}

		public TimeSpan WorkTime
		{
			get;
			private set;
		}

		public Interval Bounds
		{
			get;
			private set;
		}

		public DeviceWorkInterval[] Works
		{
			get;
			private set;
		}

		public List<Interval> Intervals
		{
			get;
			private set;
		}

		public ManualInterval[] DeletionIntervals
		{
			get;
			private set;
		}

		public Interval VisibleBounds { get; }

		public TimeSpan StartEndDiff { get; }

		public DeviceWorkIntervalLookup(ClientWorkTimeHistory history, DateTime startDateDay)
		{
			DebugEx.EnsureBgThread();
			var source = DeviceWorkInterval.GetWorks(history);
			Works = source.ToArray();
			WorksByDevice = new Dictionary<DeviceType, Dictionary<long, List<DeviceWorkInterval>>>();
			var workIdDict = new Dictionary<int, Dictionary<DeviceType, List<DeviceWorkInterval>>>();
			var concatenator = new IntervalConcatenator();
			var concatenatorWithDeleted = new IntervalConcatenator();
			foreach (var element in Works)
			{
				if (!WorksByDevice.ContainsKey(element.DeviceType))
				{
					WorksByDevice.Add(element.DeviceType, new Dictionary<long, List<DeviceWorkInterval>>());
				}

				if (!WorksByDevice[element.DeviceType].ContainsKey(element.DeviceId))
				{
					WorksByDevice[element.DeviceType].Add(element.DeviceId, new List<DeviceWorkInterval>());
				}

				if (!workIdDict.ContainsKey(element.WorkId))
				{
					workIdDict.Add(element.WorkId, new Dictionary<DeviceType, List<DeviceWorkInterval>>());
				}

				if (!workIdDict[element.WorkId].ContainsKey(element.DeviceType))
				{
					workIdDict[element.WorkId].Add(element.DeviceType, new List<DeviceWorkInterval>());
				}

				WorksByDevice[element.DeviceType][element.DeviceId].Add(element);
				workIdDict[element.WorkId][element.DeviceType].Add(element);
				if (!element.IsDeleted) concatenator.Add(element.StartDate, element.EndDate);
				concatenatorWithDeleted.Add(element.StartDate, element.EndDate);
			}

			WorkTimeById = new Dictionary<int, Dictionary<DeviceType, TimeSpan>>();
			foreach (var workId in workIdDict.Keys)
			{
				WorkTimeById.Add(workId, new Dictionary<DeviceType, TimeSpan>());
				foreach (var deviceType in workIdDict[workId].Keys)
				{
					WorkTimeById[workId][deviceType] = GetDisjointLength(workIdDict[workId][deviceType]);
				}
			}

			if (history.ManualIntervals != null)
			{
				DeletionIntervals = history.ManualIntervals.Where(
					x => x.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval
					     || x.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteInterval
					     || x.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteIvrInterval
					     || x.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteMobileInterval).ToArray();
			}
			else
			{
				DeletionIntervals = new ManualInterval[0];
			}

			Intervals = concatenator.GetIntervals().Select(x => new Interval(x.StartDate, x.EndDate)).ToList();
			var boundary = concatenatorWithDeleted.GetBoundaries();
			Bounds = boundary != null ? new Interval(boundary.Value.StartDate, boundary.Value.EndDate) : null;
			WorkTime = TimeSpan.FromMilliseconds(history.TotalTimeInMs);
			StartEndDiff = history.StartEndDiffInMs.HasValue ? TimeSpan.FromMilliseconds(history.StartEndDiffInMs.Value) : TimeSpan.Zero;
			VisibleBounds = history.StartTimeInMs.HasValue && history.EndTimeInMs.HasValue ? new Interval(ConvertToLocalTimezone(history.StartTimeInMs.Value, startDateDay), ConvertToLocalTimezone(history.EndTimeInMs.Value, startDateDay)) : null;
		}

		private static DateTime ConvertToLocalTimezone(long source, DateTime day)
		{
			var time = new DateTime(day.Year, day.Month, day.Day).AddMilliseconds(source);
			return TimeZoneInfo.ConvertTime(time, ConfigManager.TimeZoneWeb, TimeZoneInfo.Local);
		}

		private static TimeSpan GetDisjointLength(IEnumerable<DeviceWorkInterval> intervals)
		{
			var concatenator = new IntervalConcatenator();
			foreach (var element in intervals)
			{
				if (!element.IsIncludedInWorkTime) continue;
				concatenator.Add(element.StartDate, element.EndDate);
			}

			return concatenator.Duration();
		}

		public TimeSpan GetDeviceLength(DeviceType type)
		{
			if (!WorksByDevice.ContainsKey(type)) return new TimeSpan(0);
			return GetDisjointLength(WorksByDevice[type].SelectMany(x => x.Value));
		}

		public TimeSpan GetDeviceLength(int workId, DeviceType deviceType)
		{
			if (!WorkTimeById.ContainsKey(workId)) return new TimeSpan(0);
			if (!WorkTimeById[workId].ContainsKey(deviceType)) return new TimeSpan(0);
			return WorkTimeById[workId][deviceType];
		}

		public TimeSpan GetWorkLength(int workId)
		{
			return GetDisjointLength(WorksByDevice.SelectMany(x => x.Value).Select(x => x.Value).SelectMany(x => x).Where(x => x.WorkId == workId));
		}
	}
}
