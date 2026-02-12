using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public static class WorkTimeModificationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static WorkTimeModifications GetDeleteWorkModifications(DeviceWorkInterval original, string comment)
		{
			log.DebugFormat("Delete worktime - workId: {0} device: {1} interval: {2} comment: {3}", original.WorkId, original.DeviceType, original, comment);
			var mods = new List<ManualIntervalModification>();
			if (original.DeviceType != DeviceType.Manual && original.DeviceType != DeviceType.Meeting)
			{
				mods.Add(new ManualIntervalModification()
				{
					NewItem = new ManualInterval()
					{
						ManualWorkItemType = GetDeletionItem(original.DeviceType),
						StartDate = original.StartDate,
						EndDate = original.EndDate,
						Comment = comment
					}
				});
			}
			else
			{
				mods.Add(new ManualIntervalModification()
				{
					OriginalItem = original.OriginalInterval,
					NewItem = null
				});
			}

			return new WorkTimeModifications()
			{
				Comment = comment,
				ManualIntervalModifications = mods
			};
		}

		public static IEnumerable<ManualIntervalModification> GetUndeleteInterval(ManualInterval manualInterval, DeviceWorkInterval recoverInterval)
		{
			var interval = new Interval(manualInterval.StartDate, manualInterval.EndDate);
			if (interval.IsNonOverlapping(recoverInterval)) yield break;
			var res = interval.Subtract(recoverInterval).ToArray();
			if (res.Length == 0)
			{
				yield return new ManualIntervalModification()
				{
					OriginalItem = manualInterval,
					NewItem = null
				};

				yield break;
			}

			if (res.Length > 0)
			{
				yield return new ManualIntervalModification()
				{
					OriginalItem = manualInterval,
					NewItem = new ManualInterval()
					{
						ManualWorkItemType = manualInterval.ManualWorkItemType,
						StartDate = res[0].StartDate,
						EndDate = res[0].EndDate,
						Comment = manualInterval.Comment,
					}
				};
			}

			if (res.Length > 1)
			{
				yield return new ManualIntervalModification()
				{
					NewItem = new ManualInterval()
					{
						ManualWorkItemType = manualInterval.ManualWorkItemType,
						StartDate = res[1].StartDate,
						EndDate = res[1].EndDate,
						Comment = manualInterval.Comment,
					}
				};
			}
		}

		public static IEnumerable<ManualIntervalModification> GetRemoveWorkInterval(DeviceWorkInterval interval, Interval removeInterval, string comment)
		{
			if (interval.IsDeleted || !interval.IsVisible) yield break;
			if (interval.IsNonOverlapping(removeInterval)) yield break;
			var res = interval.Subtract(removeInterval).ToArray();
			if (interval.DeviceType == DeviceType.Manual || interval.DeviceType == DeviceType.Meeting)
			{
				if (res.Length == 0)
				{
					yield return new ManualIntervalModification()
					{
						OriginalItem = interval.OriginalInterval,
						NewItem = null
					};

					yield break;
				}

				if (res.Length > 0)
				{
					yield return new ManualIntervalModification()
					{
						OriginalItem = interval.OriginalInterval,
						NewItem = new ManualInterval()
						{
							ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
							StartDate = res[0].StartDate,
							EndDate = res[0].EndDate,
							Comment = comment,
							WorkId = interval.WorkId,
						}
					};
				}

				if (res.Length > 1)
				{
					yield return new ManualIntervalModification()
					{
						NewItem = new ManualInterval()
						{
							ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
							StartDate = res[1].StartDate,
							EndDate = res[1].EndDate,
							Comment = comment,
							WorkId = interval.WorkId,
						}
					};
				}
			}
		}

		public static IEnumerable<ManualIntervalModification> GetModifyIntervalModification(DeviceWorkInterval original, int newWorkId, Interval modifyInterval, string comment)
		{
			if (original.IsDeleted || !original.IsVisible) yield break;
			var common = original.Intersect(modifyInterval);
			if (common == null) yield break;
			var res = original.Subtract(modifyInterval).ToArray();
			if (original.DeviceType == DeviceType.Manual || original.DeviceType == DeviceType.Meeting)
			{
				if (res.Length == 0) // Fully overlapping
				{
					yield return new ManualIntervalModification
					{
						OriginalItem = original.OriginalInterval,
						NewItem = new ManualInterval
						{
							ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
							StartDate = original.StartDate,
							EndDate = original.EndDate,
							WorkId = newWorkId,
							Comment = comment,
						}
					};

					yield break;
				}

				Debug.Assert(res.Length > 0);
				Debug.Assert(res[0].StartDate <= res[0].EndDate);
				// Partial overlap or split -> Move part
				yield return new ManualIntervalModification
				{
					OriginalItem = original.OriginalInterval,
					NewItem = new ManualInterval
					{
						ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
						StartDate = res[0].StartDate,
						EndDate = res[0].EndDate,
						Comment = comment,
						WorkId = original.WorkId,
					}
				};

				if (res.Length > 1) // Split, first part moved, create second part
				{
					Debug.Assert(res[1].StartDate <= res[1].EndDate);
					yield return new ManualIntervalModification
					{
						NewItem = new ManualInterval()
						{
							ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
							StartDate = res[1].StartDate,
							EndDate = res[1].EndDate,
							Comment = comment,
							WorkId = original.WorkId
						}
					};
				}
			}
			else
			{
				yield return new ManualIntervalModification()
				{
					NewItem = new ManualInterval()
					{
						ManualWorkItemType = GetDeletionItem(original.DeviceType),
						StartDate = common.StartDate,
						EndDate = common.EndDate,
						Comment = comment,
					}
				};
			}

			yield return new ManualIntervalModification()
			{
				NewItem = new ManualInterval()
				{
					ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
					StartDate = common.StartDate,
					EndDate = common.EndDate,
					Comment = comment,
					WorkId = newWorkId
				}
			};
		}

		public static WorkTimeModifications GetModifyWorkModifications(DeviceWorkInterval original, WorkDataWithParentNames newWork, IEnumerable<Interval> intervals, string comment)
		{
			Debug.Assert(!original.IsDeleted);
			var newWorkId = (newWork != null && newWork.WorkData != null && newWork.WorkData.Id != null)
				? newWork.WorkData.Id.Value
				: original.WorkId;
			var intervalArr = intervals.ToArray();
			log.DebugFormat("Modify worktime - id: {0} device: {1} interval: {2} newId: {3} newIntervals: [{4}] comment: {5}", original.WorkId, original.DeviceType, original, newWorkId, string.Join(", ", intervalArr.Select(x => x.ToString()).ToArray()), comment);
			var operationList = new List<ManualIntervalModification>();
			if (original.DeviceType == DeviceType.Meeting || original.DeviceType == DeviceType.Manual)
			{
				operationList.Add(new ManualIntervalModification()
				{
					OriginalItem = original.OriginalInterval,
					NewItem = new ManualInterval()
					{
						ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
						StartDate = intervalArr.First().StartDate,
						EndDate = intervalArr.First().EndDate,
						WorkId = newWorkId,
						Comment = comment,
					}
				});

				operationList.AddRange(intervalArr.Skip(1).Select(interval => new ManualIntervalModification()
				{
					NewItem = new ManualInterval()
					{
						ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
						StartDate = interval.StartDate,
						EndDate = interval.EndDate,
						WorkId = newWorkId,
						Comment = comment,
					}
				}));
			}
			else
			{
				if (original.WorkId != newWorkId)
				{
					operationList.Add(new ManualIntervalModification()
					{
						NewItem = new ManualInterval()
						{
							ManualWorkItemType = GetDeletionItem(original.DeviceType),
							StartDate = original.StartDate,
							EndDate = original.EndDate,
							WorkId = newWorkId,
							Comment = comment,
						}
					});

					operationList.AddRange(intervalArr.Select(x => new ManualIntervalModification()
					{
						NewItem = new ManualInterval()
						{
							ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
							StartDate = x.StartDate,
							EndDate = x.EndDate,
							WorkId = newWorkId,
							Comment = comment,
						}
					}));
				}
				else
				{
					var removeRange = original.Remove(intervalArr);
					var addRange = Interval.Remove(intervalArr, new[] { original });
					operationList.AddRange(removeRange.Select(x => new ManualIntervalModification()
					{
						NewItem = new ManualInterval()
						{
							ManualWorkItemType = GetDeletionItem(original.DeviceType),
							StartDate = x.StartDate,
							EndDate = x.EndDate,
							WorkId = newWorkId,
							Comment = comment,
						}
					}));

					operationList.AddRange(addRange.Select(x => new ManualIntervalModification()
					{
						NewItem = new ManualInterval()
						{
							ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
							StartDate = x.StartDate,
							EndDate = x.EndDate,
							WorkId = newWorkId,
							Comment = comment,
						}
					}));
				}
			}

			return new WorkTimeModifications
			{
				Comment = comment,
				ManualIntervalModifications = operationList
			};
		}

		public static WorkTimeModifications GetCreateWorkModifications(WorkDataWithParentNames work, IEnumerable<Interval> intervals, string comment)
		{
			Debug.Assert(work != null && work.WorkData != null && work.WorkData.Id != null);
			var commands = new WorkTimeModifications()
			{
				Comment = comment,
				ManualIntervalModifications = intervals.Select(x => new ManualIntervalModification()
				{
					NewItem = new ManualInterval()
					{
						ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
						StartDate = x.StartDate,
						EndDate = x.EndDate,
						WorkId = work.WorkData.Id.Value,
						Comment = comment
					}
				}).ToList()
			};

			return commands;
		}

		private static ManualWorkItemTypeEnum GetDeletionItem(DeviceType device)
		{
			switch (device)
			{
				case DeviceType.Computer:
					return ManualWorkItemTypeEnum.DeleteComputerInterval;
				case DeviceType.Ivr:
					return ManualWorkItemTypeEnum.DeleteIvrInterval;
				case DeviceType.Mobile:
					return ManualWorkItemTypeEnum.DeleteMobileInterval;
				default:
					throw new NotImplementedException();
			}
		}

	}
}
