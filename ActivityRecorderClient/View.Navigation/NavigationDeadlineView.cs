using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationDeadlineView : NavigationBase
	{
		public NavigationDeadlineView(INavigator navigator)
			: base(LocationKey.Deadline, navigator)
		{
			Icon = Resources.deadline;
			Render = RenderHint.Remaining;
			IsEditable = false;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
		}

		public override void Localize()
		{
			Name = Labels.NavigationDeadline;
		}

		protected override void Dispose(bool disposing)
		{
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			ClientMenuLookup menu = MenuQuery.Instance.ClientMenuLookup.Value;
			IEnumerable<WorkDataWithParentNames> flatWorkData = MenuHelper.FlattenDistinctWorkDataThatHasId(menu.ClientMenu);
			return flatWorkData
				.Where(n => n.WorkData.IsVisibleInMenu && n.WorkData != null && n.WorkData.EndDate.HasValue)
				.OrderBy(n => n.WorkData.EndDate.Value)
				.ThenByDescending(n => n.WorkData.Priority ?? 0).Take(MenuMaxSize)
				.Select(x => LocationKey.CreateFrom(x.WorkData)).ToArray();
		}

		private void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}