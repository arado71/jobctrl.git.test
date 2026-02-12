using System;
using System.Collections.Generic;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationPriorityView : NavigationBase
	{
		public NavigationPriorityView(INavigator navigator)
			: base(LocationKey.Priority, navigator)
		{
			Icon = Resources.priority;
			IsEditable = false;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
			Render = RenderHint.Priority;
		}

		public override void Localize()
		{
			Name = Labels.NavigationPriority;
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
				.Where(n => n.WorkData.IsVisibleInMenu && MenuBuilderHelper.HasPriority(n.WorkData))
				// ReSharper disable once PossibleInvalidOperationException - HasPriority() call makes sure workdata has priority value
				.OrderByDescending(n => n.WorkData.Priority.Value).Take(MenuMaxSize)
				.Select(x => LocationKey.CreateFrom(x.WorkData)).ToArray();
		}

		private void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}