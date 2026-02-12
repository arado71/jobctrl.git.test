using System;
using System.Collections.Generic;
using System.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationProgressView : NavigationBase
	{
		public NavigationProgressView(INavigator navigator)
			: base(LocationKey.Progress, navigator)
		{
			Render = RenderHint.Progress;
			Icon = Resources.progress;
			IsEditable = false;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
			MenuQuery.Instance.SimpleWorkTimeStats.Changed += HandleMenuChanged;
		}

		public override void Localize()
		{
			Name = Labels.NavigationProgress;
		}

		protected override void Dispose(bool disposing)
		{
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuChanged;
			MenuQuery.Instance.SimpleWorkTimeStats.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			ClientMenuLookup menu = MenuQuery.Instance.ClientMenuLookup.Value;
			SimpleWorkTimeStats simpleStats = MenuQuery.Instance.SimpleWorkTimeStats.Value;
			IEnumerable<WorkDataWithParentNames> flatWorkData = MenuHelper.FlattenDistinctWorkDataThatHasId(menu.ClientMenu);
			return flatWorkData
				.Where(n => n.WorkData.IsVisibleInMenu && MenuBuilderHelper.HasTargetTotalWorkTime(n.WorkData))
				.OrderByDescending(n => MenuBuilderHelper.GetTargetWorkTimePct(n.WorkData, simpleStats) ?? -1)
				.Take(MenuMaxSize)
				.Select(x => LocationKey.CreateFrom(x.WorkData)).ToArray();
		}

		private void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}