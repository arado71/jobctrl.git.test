using System;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationRecentView : NavigationBase
	{
		public NavigationRecentView(INavigator navigator)
			: base(LocationKey.Recent, navigator)
		{
			Icon = Resources.recent;
			Render = RenderHint.Long;
			IsEditable = false;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
			RecentHelper.RecentChanged += HandleMenuChanged;
		}

		public override void Localize()
		{
			Name = Labels.NavigationRecent;
		}

		protected override void Dispose(bool disposing)
		{
			RecentHelper.RecentChanged -= HandleMenuChanged;
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			return RecentHelper.GetRecents().Where(x => x.WorkData.IsVisibleInMenu)
				.Select(x => LocationKey.CreateFrom(x.WorkData)).ToArray();
		}

		private void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}