using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationProject : NavigationBase
	{
		public NavigationProject(INavigator navigator, WorkDataWithParentNames work)
			: base(LocationKey.CreateFrom(work.WorkData), navigator)
		{
			Debug.Assert(work.WorkData.ProjectId != null);
			Name = work.WorkData.Name;
			Path = work.ParentNames;
			Render = RenderHint.Short;
			CanFavorite = true;
			IsEditable = false;
			isFavorite = FavoritesHelper.IsFavorite(Id);
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuChanged;
		}

		public override void Localize()
		{
		}

		protected override void Dispose(bool disposing)
		{
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuChanged;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			ClientMenuLookup menu = MenuQuery.Instance.ClientMenuLookup.Value;
			if (!menu.ProjectDataById.ContainsKey(Id))
			{
				return new LocationKey[0];
			}
			return menu.ProjectDataById[Id].WorkData.Children
				.Where(x => x.Id.HasValue || x.ProjectId.HasValue) //handle buggy menu
				.Where(IsVisibleInMenu)
				.Select(work => LocationKey.CreateFrom(work))
				.ToArray();
		}

		public static bool IsVisibleInMenu(WorkData workData)
		{
			if (workData.Id != null)
			{
				return (ConfigManager.LocalSettingsForUser.ShowDynamicWorks || !MenuQuery.Instance.ClientMenuLookup.Value.IsDynamicWork(workData.Id.Value)) && workData.IsVisibleInMenu;
			}
			
			Debug.Assert(workData.ProjectId != null);
			return workData.IsVisibleInMenu && workData.Children.Any(IsVisibleInMenu);
		}

		protected void HandleMenuChanged(object sender, EventArgs e)
		{
			Children = GetChildren();
		}
	}
}