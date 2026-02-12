using System;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationAllView : NavigationBase
	{
		public NavigationAllView(INavigator navigator)
			: base(LocationKey.All, navigator)
		{
			Icon = Resources.hierarchy;
			IsEditable = false;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
			Render = RenderHint.Short;
		}

		public override void Localize()
		{
			Name = Labels.NavigationAll;
		}

		protected override void Dispose(bool disposing)
		{
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			ClientMenuLookup menu = MenuQuery.Instance.ClientMenuLookup.Value;
			if (menu.ClientMenu == null || menu.ClientMenu.Works == null)
			{
				return new LocationKey[0];
			}

			return menu.ClientMenu.Works
				.Where(x => x.Id.HasValue || x.ProjectId.HasValue) //handle buggy menu
				.Where(NavigationProject.IsVisibleInMenu)
				.Select(x => LocationKey.CreateFrom(x))
				.ToArray();
		}

		private void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}