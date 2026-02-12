using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu.Management;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Menu
{
	public static class MenuHelper
	{
		public static bool MenuContainsId(ClientMenu menu, int id)
		{
			if (menu == null || menu.Works == null) return false;
			foreach (var work in menu.Works)
			{
				if (WorkDataContainsId(work, id)) return true;
			}
			return false;
		}

		private static bool WorkDataContainsId(WorkData work, int id)
		{
			if (work == null) return false;
			if (work.Id.HasValue && work.Id.Value == id) return true;
			if (work.Children == null) return false;
			foreach (var childWork in work.Children)
			{
				if (WorkDataContainsId(childWork, id)) return true;
			}
			return false;
		}

		public static IEnumerable<WorkDataWithParentNames> FlattenWithParentNames(ClientMenu menu)
		{
			if (menu == null || menu.Works == null) yield break;
			foreach (var wd in FlattenWithParentNames(menu.Works, new List<string>(), null))
			{
				yield return wd;
			}
		}

		public static IEnumerable<WorkDataWithParentNames> FlattenDistinctWorkDataThatHasId(ClientMenu menu)
		{
			return FlattenDistinctWorkDataThatHasId(menu, true);
		}

		public static IEnumerable<WorkDataWithParentNames> FlattenDistinctWorkDataThatHasId(ClientMenu menu, bool includeManualAddWorks)
		{
			return FlattenWithParentNames(menu)
				.Where(n => n.WorkData.Id.HasValue) //we only care about clickable items here
				.Where(n => includeManualAddWorks || !n.WorkData.ManualAddWorkDuration.HasValue)
				.Distinct(WorkDataWithParentNames.WorkDataIdComparer);
		}

		public static IEnumerable<WorkDataWithParentNames> FlattenDistinctWorkDataThatHasProjectId(ClientMenu menu)
		{
			return FlattenWithParentNames(menu)
				.Where(n => n.WorkData.ProjectId.HasValue)
				.Distinct(WorkDataWithParentNames.WorkDataProjectIdComparer);
		}


		private static IEnumerable<WorkDataWithParentNames> FlattenWithParentNames(IEnumerable<WorkData> workDatas, List<string> currentParentNames, int? parentId)
		{
			foreach (var workData in workDatas)
			{
				if (workData == null) continue;
				yield return new WorkDataWithParentNames { WorkData = workData, ParentNames = currentParentNames, ParentId = parentId };
				if (workData.Children == null) continue;
				foreach (var childWorkData in FlattenWithParentNames(workData.Children, currentParentNames.Concat(new[] { workData.Name }).ToList(), workData.ProjectId.HasValue ? workData.ProjectId.Value : (int?)null))
				{
					yield return childWorkData;
				}
			}
		}

		public static MenuDifference GetMenuDifference(ClientMenuLookup newMenuLookup, ClientMenuLookup oldMenuLookup)
		{
			var oldWorksDict = oldMenuLookup.WorkDataById;
			var newWorksDict = newMenuLookup.WorkDataById;

			var newWorks = newWorksDict.Where(n => !oldWorksDict.ContainsKey(n.Key)).Select(n => n.Value).ToList();
			var delWorks = oldWorksDict.Where(n => !newWorksDict.ContainsKey(n.Key)).Select(n => n.Value).ToList();
			var joined = from n in newWorksDict.Values
						 join o in oldWorksDict.Values on n.WorkData.Id.Value equals o.WorkData.Id.Value
						 select new MenuDifference.WorkDifference { OldWork = o, NewWork = n };
			return new MenuDifference(newWorks, delWorks, joined);
		}

		public static MessageWithActions GetMenuChangeString(ClientMenuLookup newMenuLookup, ClientMenuLookup oldMenuLookup, MenuDifference diff, Func<WorkData, ClientMenuLookup, bool> ignoreWorkData, IWorkManagementService workManagementService, out bool shortDisplay)
		{
			var changedWorks = diff.ExistingWorks
				.Select(oldNewWork => new
				{
					Work = oldNewWork.NewWork,
					ChangeString =
						string.Join(Environment.NewLine, new[] {
							oldNewWork.OldWork.WorkData.Name != oldNewWork.NewWork.WorkData.Name
								? "      " + Labels.WorkData_Name + ": " + oldNewWork.OldWork.WorkData.Name + " -> " + oldNewWork.NewWork.WorkData.Name
								: null,
							oldNewWork.OldWork.WorkData.CategoryId != oldNewWork.NewWork.WorkData.CategoryId
								? "      " + Labels.WorkData_Category + ": " + 
									(oldNewWork.OldWork.WorkData.CategoryId.HasValue ? oldMenuLookup.AllCategoriesById[oldNewWork.OldWork.WorkData.CategoryId.Value].Name : "") + " -> " 
									+ (oldNewWork.NewWork.WorkData.CategoryId.HasValue ? newMenuLookup.AllCategoriesById[oldNewWork.NewWork.WorkData.CategoryId.Value].Name : "")
								: null,
							oldNewWork.OldWork.WorkData.Priority != oldNewWork.NewWork.WorkData.Priority
								? "      " + Labels.WorkData_Priority + ": " + oldNewWork.OldWork.WorkData.Priority + " -> " + oldNewWork.NewWork.WorkData.Priority
								: null,
							oldNewWork.OldWork.WorkData.StartDate != oldNewWork.NewWork.WorkData.StartDate
								? "      " + Labels.WorkData_StartDate + ": " + oldNewWork.OldWork.WorkData.StartDate.ToShortDateString() + " -> " + oldNewWork.NewWork.WorkData.StartDate.ToShortDateString()
								: null,
							oldNewWork.OldWork.WorkData.EndDate != oldNewWork.NewWork.WorkData.EndDate
								? "      " + Labels.WorkData_EndDate + ": " + oldNewWork.OldWork.WorkData.EndDate.ToShortDateString() + " -> " + oldNewWork.NewWork.WorkData.EndDate.ToShortDateString()
								: null,
							oldNewWork.OldWork.WorkData.TargetTotalWorkTime != oldNewWork.NewWork.WorkData.TargetTotalWorkTime
								? "      " + Labels.WorkData_TargetHours + ": " + oldNewWork.OldWork.WorkData.TargetTotalWorkTime.ToHourMinuteString() + " -> " + oldNewWork.NewWork.WorkData.TargetTotalWorkTime.ToHourMinuteString()
								: null,
						}.Where(n => n != null).ToArray())
				})
				.Where(n => !string.IsNullOrEmpty(n.ChangeString))
				.ToList();

			var sb = new MessageWithActions();
			sb.Append(Labels.NotificationMenuChangedBody + " (" + DateTime.Now.ToDateWithHourMinuteSecondString() + ")");
			if (diff.NewWorks.Length > 0)
			{
				sb.AppendLine();
				sb.Append(diff.NewWorks.Length == 1 ? Labels.NewWork : Labels.NewWorks).AppendLine(":");
				foreach (var newItem in diff.NewWorks)
				{
					sb.Append(" + ");
					sb.Append(newItem.ToString(), () => workManagementService.DisplayUpdateWorkGui(newItem.WorkData));
					sb.AppendLine();
				}
			}

			if (diff.DeletedWorks.Length > 0)
			{
				sb.AppendLine();
				sb.Append(diff.DeletedWorks.Length == 1 ? Labels.DeletedWork : Labels.DeletedWorks).AppendLine(":");
				foreach (var deletedItem in diff.DeletedWorks)
				{
					sb.Append(" - ");
					sb.Append(deletedItem.ToString(), () => workManagementService.DisplayUpdateWorkGui(deletedItem.WorkData));
					sb.AppendLine();
				}
			}

			if (changedWorks.Count > 0)
			{
				sb.AppendLine();
				sb.Append(changedWorks.Count == 1 ? Labels.ChangedWork : Labels.ChangedWorks).AppendLine(":");
				foreach (var changedItem in changedWorks)
				{
					sb.Append(" * ");
					sb.Append(changedItem.Work.ToString(), () => workManagementService.DisplayUpdateWorkGui(changedItem.Work.WorkData));
					sb.AppendLine();
					sb.Append(changedItem.ChangeString);
					sb.AppendLine();
				}
			}

			shortDisplay = diff.NewWorks.All(n => ignoreWorkData(n.WorkData, newMenuLookup))
				&& diff.DeletedWorks.All(n => ignoreWorkData(n.WorkData, oldMenuLookup))
				&& changedWorks.All(n => ignoreWorkData(n.Work.WorkData, newMenuLookup))
				;

			return sb;
		}

		public static List<HashSet<int>> GetWorkIdsByProject(ClientMenu menu)
		{
			var result = new List<HashSet<int>>();
			if (menu != null)
			{
				GetWorkIdsByProject(menu.Works, result);
			}
			return result;
		}

		private static void GetWorkIdsByProject(IEnumerable<WorkData> worksData, List<HashSet<int>> result)
		{
			if (worksData == null) return;
			var curr = new HashSet<int>(worksData.Where(n => n.Id.HasValue).Select(n => n.Id.Value));
			if (curr.Count > 0) result.Add(curr);
			foreach (var workData in worksData.Where(n => n.Children != null && n.Children.Count > 0))
			{
				GetWorkIdsByProject(workData.Children, result);
			}
		}
	}
}
