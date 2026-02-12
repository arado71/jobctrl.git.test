using System;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationRecentClosedView : NavigationBase
	{
		public NavigationRecentClosedView(INavigator navigator)
			: base(LocationKey.RecentClosed, navigator)
		{
			Icon = Resources.closed_task;
			Render = RenderHint.Short;
			IsEditable = false;
			RecentClosedHelper.Changed += HandleMenuChanged;
		}

		public override void Localize()
		{
			Name = Labels.NavigationRecentClosed;
		}

		protected override void Dispose(bool disposing)
		{
			RecentClosedHelper.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			return RecentClosedHelper.GetRecents().Select(x => LocationKey.CreateClosed(x)).ToArray();
		}

		private void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}