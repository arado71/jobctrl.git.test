using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderService;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public abstract class WorkTimeService : IWorkTimeService
	{
		private const int TooMuchAddLimit = 10;
		private static readonly TimeSpan MinimumAddLimit = TimeSpan.FromSeconds(5);
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(5);
		private readonly CachedFunc<Interval, DeviceWorkIntervalLookup> dayCache = null;
		private TimeSpan? dayStartOffset = null;
		private readonly IWorkTimeQuery workTimeHistory;
		private readonly WorkNameProvider workNameProvider;
		private bool isModificationApprovalNeeded;
		private TimeSpan tooOldLimit = TimeSpan.FromHours(72);

		protected WorkTimeService(IWorkTimeQuery workTimeHistory)
		{
			this.workTimeHistory = workTimeHistory;
			workNameProvider = new WorkNameProvider(x => workTimeHistory.GetWorkNames(ConfigManager.UserId, x));
			dayCache = new CachedFunc<Interval, DeviceWorkIntervalLookup>(GetDeviceWorkIntervalLookup, cacheExpiration, true);
		}

		public abstract void ShowModification(DateTime? localDay = null);
		public abstract void ShowModifyWork(DeviceWorkInterval workInterval);
		public abstract void ShowModifyInterval(Interval localInterval);

		public GeneralResult<Interval> GetLocalDayInterval(DateTime localDay)
		{
			Debug.Assert(localDay.Date == localDay);
			DebugEx.EnsureBgThread();
			try
			{
				var off = GetDayStartOffset();
				return new GeneralResult<Interval> { Result = new Interval { StartDate = (localDay.Date + off).FromLocalToUtc(), EndDate = (localDay.Date.AddDays(1.0) + off).FromLocalToUtc() } };
			}
			catch (Exception e)
			{
				log.Error("Error in GetDayInterval", e);
				return new GeneralResult<Interval> { Exception = e };
			}
		}

		public GeneralResult<IEnumerable<WorkOrProjectWithParentNames>> GetWorkOrProjectWithParentNames(IEnumerable<int> workIds)
		{
			DebugEx.EnsureBgThread();
			try
			{
				return new GeneralResult<IEnumerable<WorkOrProjectWithParentNames>>
				{
					Result = workNameProvider.GetWorkOrProjectWithParentNames(workIds)
				};
			}
			catch (Exception e)
			{
				log.Error("Error in GetWorkOrProjectWithParentNames", e);
				return new GeneralResult<IEnumerable<WorkOrProjectWithParentNames>> { Exception = e };
			}
		}

		public GeneralResult<IList<Interval>> GetFreeIntervals(Interval interval, Interval exceptionInterval = null)
		{
			DebugEx.EnsureBgThread();
			try
			{
				var overlapping = GetDeviceWorkIntervals(interval).Where(otherInterval => !otherInterval.Equals(exceptionInterval)).ToList();
				return new GeneralResult<IList<Interval>> { Result = interval.Remove(overlapping.Cast<Interval>()) };
			}
			catch (Exception e)
			{
				log.Error("Error in GetFreeIntervals", e);
				return new GeneralResult<IList<Interval>> { Exception = e };
			}
		}

		public GeneralResult<bool> ModifyInterval(Interval interval, WorkDataWithParentNames newWork, string comment, bool force = false)
		{
			DebugEx.EnsureBgThread();
			Debug.Assert(interval.StartDate < interval.EndDate);
			Debug.Assert(newWork != null && newWork.WorkData != null && newWork.WorkData.Id != null);
			try
			{
				if (interval.Duration.TotalHours >= 24) throw new ValidationException(new[] { new ValidationResult(Severity.Error, Labels.Worktime_TooLong) });
				var mods = GetDeviceWorkIntervals(interval)
					.SelectMany(x =>
						WorkTimeModificationHelper.GetModifyIntervalModification(x, newWork.WorkData.Id.Value, interval, comment)).ToList();
				var commands = new WorkTimeModifications()
				{
					Comment = comment,
					ManualIntervalModifications = mods,
				};

				ValidateModifications(commands, force);
				workTimeHistory.Modify(commands, ConfigManager.UserId);
				RefreshCache(CalculateModificationBounds(commands));

				return new GeneralResult<bool> { Result = true };
			}
			catch (Exception e)
			{
				log.Error("Error in ModifyInterval", e);
				return new GeneralResult<bool> { Exception = e };
			}
		}

		public GeneralResult<bool> ModifyWork(DeviceWorkInterval originalInterval, WorkDataWithParentNames workData,
			IEnumerable<Interval> intervals, string comment, bool force = false)
		{
			DebugEx.EnsureBgThread();
			Debug.Assert(originalInterval.IsVisible && !originalInterval.IsDeleted);
			try
			{
				var intArr = intervals.ToArray();
				if (Interval.GetLength(intArr).TotalHours >= 24) throw new ValidationException(new[] { new ValidationResult(Severity.Error, Labels.Worktime_TooLong) });
				var commands = WorkTimeModificationHelper.GetModifyWorkModifications(originalInterval, workData, intArr, comment);

				ValidateModifications(commands, force);
				workTimeHistory.Modify(commands, ConfigManager.UserId);
				RefreshCache(CalculateModificationBounds(commands));

				return new GeneralResult<bool> { Result = true };
			}
			catch (Exception e)
			{
				log.Error("Error in ModifyWork", e);
				return new GeneralResult<bool> { Exception = e };
			}
		}

		public GeneralResult<bool> DeleteWork(DeviceWorkInterval originalInterval, string comment, bool force = false)
		{
			DebugEx.EnsureBgThread();
			Debug.Assert(originalInterval.IsVisible && !originalInterval.IsDeleted);
			try
			{
				var commands = WorkTimeModificationHelper.GetDeleteWorkModifications(originalInterval, comment);

				ValidateModifications(commands, force);
				workTimeHistory.Modify(commands, ConfigManager.UserId);
				RefreshCache(CalculateModificationBounds(commands));

				return new GeneralResult<bool> { Result = true };
			}
			catch (Exception e)
			{
				log.Error("Error in DeleteWork", e);
				return new GeneralResult<bool> { Exception = e };
			}
		}

		public GeneralResult<bool> CreateWork(WorkDataWithParentNames work, Interval interval, string comment, bool force = false)
		{
			DebugEx.EnsureBgThread();
			Debug.Assert(work != null && work.WorkData != null && work.WorkData.Id != null);
			try
			{
				if (interval.Duration.TotalHours >= 24) throw new ValidationException(new[] { new ValidationResult(Severity.Error, Labels.Worktime_TooLong) });
				var validationMessages = new List<ValidationResult>();
				var intervals = interval.Remove(GetDeviceWorkIntervals(interval).Where(x => x.IsVisible && !x.IsDeleted).Cast<Interval>()).Where(x => x.Duration >= MinimumAddLimit).ToList();
				var oldIntervalLength = (interval.EndDate - interval.StartDate);
				var newIntervalLength = Interval.GetLength(intervals);
				if (newIntervalLength != oldIntervalLength)
				{
					if (newIntervalLength.Ticks == 0)
					{
						throw new ValidationException(new[] { new ValidationResult(Severity.Error, Labels.ModifyWork_NoValidInterval) });
					}

					if (!force)
					{
						var text = string.Format(Labels.ModifyWork_Overlap, oldIntervalLength.ToHourMinuteSecondString(),
							newIntervalLength.ToHourMinuteSecondString(), intervals.Count);
						validationMessages.Add(new ValidationResult(Severity.Warn, text));
					}
				}

				var intervalArr = intervals.ToArray();
				var commands = WorkTimeModificationHelper.GetCreateWorkModifications(work, intervalArr, comment);
				if (
					commands.ManualIntervalModifications.Count(
						x => x.NewItem != null && x.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork) >= TooMuchAddLimit)
				{
					throw new ValidationException(new[] { new ValidationResult(Severity.Error, Labels.Worktime_TooMuchAddError) });
				}

				validationMessages.AddRange(GetValidations(commands, force));
				if (validationMessages.Count > 0)
				{
					log.InfoFormat("Validation errors: {0}", string.Join(", ", validationMessages.Select(x => x.ToString()).ToArray()));
					throw new ValidationException(validationMessages);
				}

				workTimeHistory.Modify(commands, ConfigManager.UserId);
				RefreshCache(CalculateModificationBounds(commands));

				return new GeneralResult<bool> { Result = true };
			}
			catch (Exception e)
			{
				log.Error("Error in CreateWork", e);
				return new GeneralResult<bool> { Exception = e };
			}
		}

		public GeneralResult<bool> DeleteInterval(Interval interval, string comment, bool force = false)
		{
			DebugEx.EnsureBgThread();
			try
			{
				if (interval.Duration.TotalHours >= 24) throw new ValidationException(new[] { new ValidationResult(Severity.Error, Labels.Worktime_TooLong) });
				var mods = GetDeviceWorkIntervals(interval).SelectMany(x => WorkTimeModificationHelper.GetRemoveWorkInterval(x, interval, comment)).ToList();
				var intervalConcatenator = new IntervalConcatenator();
				intervalConcatenator.Add(interval.StartDate, interval.EndDate);
				var deletions = GetDeletions(interval).Where(x => x.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteInterval);
				foreach (var deletion in deletions)
				{
					intervalConcatenator.Remove(deletion.StartDate, deletion.EndDate);
				}

				mods.AddRange(intervalConcatenator.GetIntervals().Select(x =>
					new ManualIntervalModification
					{
						NewItem = new ManualInterval
						{
							ManualWorkItemType = ManualWorkItemTypeEnum.DeleteInterval,
							Comment = comment,
							StartDate = x.StartDate,
							EndDate = x.EndDate,
						}
					}));
				var commands = new WorkTimeModifications
				{
					Comment = comment,
					ManualIntervalModifications = mods,
				};

				ValidateModifications(commands, force);
				workTimeHistory.Modify(commands, ConfigManager.UserId);
				RefreshCache(CalculateModificationBounds(commands));

				return new GeneralResult<bool> { Result = true };
			}
			catch (Exception e)
			{
				log.Error("Error in DeleteInterval", e);
				return new GeneralResult<bool> { Exception = e };
			}
		}

		public GeneralResult<bool> UndeleteWork(DeviceWorkInterval workInterval)
		{
			DebugEx.EnsureBgThread();
			try
			{
				var mods =
					GetDeletions(workInterval)
						.SelectMany(x => WorkTimeModificationHelper.GetUndeleteInterval(x, workInterval))
						.ToList();
				var commands = new WorkTimeModifications()
				{
					Comment = "",
					ManualIntervalModifications = mods
				};
				workTimeHistory.Modify(commands, ConfigManager.UserId);
				RefreshCache(CalculateModificationBounds(commands));

				return new GeneralResult<bool> { Result = true };
			}
			catch (Exception e)
			{
				log.Error("Error in UndeleteWork", e);
				return new GeneralResult<bool> { Exception = e };
			}
		}

		public GeneralResult<DeviceWorkIntervalLookup> GetStats(Interval interval)
		{
			DebugEx.EnsureBgThread();
			try
			{
				var value = dayCache.GetOrCalculateValue(interval);
				if (value.Bounds != null && value.WorkTime == TimeSpan.Zero) dayCache.Remove(interval); // incomplete data in case of server side failure
				return new GeneralResult<DeviceWorkIntervalLookup> { Result = value };
			}
			catch (Exception e)
			{
				log.Error("Error in GetStats", e);
				return new GeneralResult<DeviceWorkIntervalLookup> { Exception = e };
			}
		}

		private Interval CalculateModificationBounds(WorkTimeModifications commands)
		{
			var oldIntervals = commands.ManualIntervalModifications.Where(x => x.OriginalItem != null)
				.Select(x => new Interval(x.OriginalItem.StartDate, x.OriginalItem.EndDate));
			var newIntervals = commands.ManualIntervalModifications.Where(x => x.NewItem != null)
							.Select(x => new Interval(x.NewItem.StartDate, x.NewItem.EndDate));
			return Interval.GetBounds(oldIntervals.Union(newIntervals));
		}

		private void ValidateModifications(WorkTimeModifications modifications, bool force)
		{
			var validationResults = GetValidations(modifications, force).ToList();
			if (
				modifications.ManualIntervalModifications.Count(
					x => x.NewItem != null && x.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork) >= TooMuchAddLimit)
			{
				validationResults.Add(new ValidationResult(Severity.Error, Labels.Worktime_TooMuchAddError));
			}

			if (validationResults.Count > 0)
			{
				log.InfoFormat("Validation errors: {0}", string.Join(", ", validationResults.Select(x => x.ToString()).ToArray()));
				throw new ValidationException(validationResults);
			}
		}

		private ValidationResult[] GetValidations(WorkTimeModifications modifications, bool force)
		{
			return modifications.ManualIntervalModifications.SelectMany(x => ValidateModification(x, force)).Distinct().ToArray();
		}

		private IEnumerable<ValidationResult> ValidateModification(ManualIntervalModification modification, bool force)
		{
			if (modification.OriginalItem != null)
			{
				if (!modification.OriginalItem.IsEditable) yield return new ValidationResult(Severity.Error, Labels.Worktime_CantEditError);
				if (modification.OriginalItem.StartDate <= DateTimeEx.UtcNow.Add(-tooOldLimit)) yield return new ValidationResult(Severity.Error, string.Format(Labels.Worktime_TooOldError, (int)tooOldLimit.TotalHours));
				if (!force) yield return new ValidationResult(Severity.Warn, Labels.Worktime_ManualModificationWarn);
			}

			if (modification.NewItem != null)
			{
				if (modification.NewItem.EndDate >= DateTimeEx.UtcNow.AddMinutes(-5)) yield return new ValidationResult(Severity.Error, Labels.Worktime_NoFuture);
				if (modification.NewItem.StartDate <= DateTimeEx.UtcNow.Add(-tooOldLimit)) yield return new ValidationResult(Severity.Error, string.Format(Labels.Worktime_TooOldError, (int)tooOldLimit.TotalHours));
			}

			if (!force && isModificationApprovalNeeded && (
				(modification.NewItem != null && modification.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)))
			{
				yield return new ValidationResult(Severity.Warn, Labels.Worktime_ApprovalWarn);
			}

		}

		private TimeSpan GetDayStartOffset()
		{
			if (dayStartOffset == null)
			{
				dayStartOffset = workTimeHistory.GetStartOfDayOffset(ConfigManager.UserId);
			}

			return dayStartOffset.Value;
		}

		private IEnumerable<ManualInterval> GetDeletions(Interval interval)
		{
			var currentLocalDate = (interval.StartDate.FromUtcToLocal() - GetDayStartOffset()).Date;
			var endLocalTime = interval.EndDate.FromUtcToLocal() - GetDayStartOffset();
			var firstInterval = true;
			while (currentLocalDate < endLocalTime)
			{
				var currentDayStart = (currentLocalDate + GetDayStartOffset()).FromLocalToUtc();
				var currentDayEnd = (currentLocalDate + GetDayStartOffset()).AddDays(1.0).FromLocalToUtc();
				var candidates = dayCache.GetOrCalculateValue(new Interval(currentDayStart, currentDayEnd)).DeletionIntervals;
				foreach (var manualInterval in candidates)
				{
					var manualAsInterval = new Interval(manualInterval.StartDate, manualInterval.EndDate);
					var isInInterval = !manualAsInterval.IsNonOverlapping(interval);
					var isDuplicateInterval = !firstInterval && manualInterval.StartDate <= currentDayStart;
					if (isInInterval && !isDuplicateInterval)
					{
						yield return manualInterval;
					}
				}

				currentLocalDate = currentLocalDate.AddDays(1);
				firstInterval = false;
			}
		}

		private IEnumerable<DeviceWorkInterval> GetDeviceWorkIntervals(Interval interval)
		{
			var currentLocalDate = (interval.StartDate.FromUtcToLocal() - GetDayStartOffset()).Date;
			var endLocalTime = interval.EndDate.FromUtcToLocal() - GetDayStartOffset();
			var firstInterval = true;
			while (currentLocalDate < endLocalTime)
			{
				var currentDayStart = (currentLocalDate + GetDayStartOffset()).FromLocalToUtc();
				var currentDayEnd = (currentLocalDate + GetDayStartOffset()).AddDays(1.0).FromLocalToUtc();
				var candidates = dayCache.GetOrCalculateValue(new Interval(currentDayStart, currentDayEnd)).Works;
				foreach (var deviceWorkInterval in candidates)
				{
					var isInInterval = !deviceWorkInterval.IsNonOverlapping(interval);
					var isDuplicateInterval = !firstInterval && deviceWorkInterval.StartDate <= currentDayStart;
					if (isInInterval && !isDuplicateInterval)
					{
						yield return deviceWorkInterval;
					}
				}

				currentLocalDate = currentLocalDate.AddDays(1);
				firstInterval = false;
			}
		}

		private void RefreshCache(Interval interval)
		{
			var currentLocalDate = (interval.StartDate.FromUtcToLocal() - GetDayStartOffset()).Date;
			var endLocalTime = interval.EndDate.FromUtcToLocal() - GetDayStartOffset();
			while (currentLocalDate < endLocalTime)
			{
				var currentDayStart = (currentLocalDate + GetDayStartOffset()).FromLocalToUtc();
				var currentDayEnd = (currentLocalDate + GetDayStartOffset()).AddDays(1.0).FromLocalToUtc();
				var dayInterval = new Interval(currentDayStart, currentDayEnd);
				dayCache.Remove(dayInterval);
				dayCache.GetOrCalculateValue(dayInterval);
				currentLocalDate = currentLocalDate.AddDays(1);
			}
		}

		private DeviceWorkIntervalLookup GetDeviceWorkIntervalLookup(Interval interval)
		{
			log.DebugFormat("Updating cache for {0}", interval);
			// todo UserID from dependency class (Composition)
			var history = workTimeHistory.GetStats(interval.StartDate, interval.EndDate, ConfigManager.UserId);
			isModificationApprovalNeeded = history.IsModificationApprovalNeeded;
			tooOldLimit = history.ModificationAgeLimit;
			return new DeviceWorkIntervalLookup(history, interval.StartDate);
		}
	}
}
