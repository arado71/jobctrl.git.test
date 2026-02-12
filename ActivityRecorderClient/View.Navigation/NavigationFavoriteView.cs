using System;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationFavoriteView : NavigationBase
	{
		public NavigationFavoriteView(INavigator navigator)
			: base(LocationKey.Favorite, navigator)
		{
			Icon = Resources.favorites;
			Reorderable = true;
			IsEditable = false;
			Render = RenderHint.Long;
			FavoritesHelper.ListChanged += HandleMenuChanged;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
		}

		public override void Localize()
		{
			Name = Labels.NavigationFavorite;
		}

		public override void Reorder(NavigationBase child, int position)
		{
			FavoritesHelper.Move(child.Id, position);
		}

		protected override void Dispose(bool disposing)
		{
			FavoritesHelper.ListChanged -= HandleMenuChanged;
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			return FavoritesHelper.GetFavorites().Where(x => x.WorkData.IsVisibleInMenu)
				.Select(x => LocationKey.CreateFrom(x.WorkData)).ToArray();
		}

		private void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}