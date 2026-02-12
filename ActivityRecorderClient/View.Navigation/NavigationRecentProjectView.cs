using System;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationRecentProjectView : NavigationBase
	{
		public NavigationRecentProjectView(INavigator navigator)
			: base(LocationKey.RecentProject, navigator)
		{
			Icon = Resources.recent_projects;
			Render = RenderHint.Long;
			IsEditable = false;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
			RecentHelper.RecentChanged += HandleMenuChanged;
		}

		public override void Localize()
		{
			Name = Labels.NavigationRecentProject;
		}

		protected override void Dispose(bool disposing)
		{
			RecentHelper.RecentChanged -= HandleMenuChanged;
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			var clientMenu = MenuQuery.Instance.ClientMenuLookup.Value;
			return
				RecentHelper.GetRecentIds()
					.Where(x => clientMenu.ProjectByWorkId.ContainsKey(x))
					.Select(x => clientMenu.ProjectByWorkId[x])
					.Distinct(WorkDataWithParentNames.WorkDataProjectIdComparer)
					.Select(x => LocationKey.CreateFrom(x.WorkData)).ToArray();
		}

		protected void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}