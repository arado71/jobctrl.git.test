using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationRootView : NavigationBase
	{
		private readonly NavigationFactory factory;
		private KeyValuePair<LocationKey, NavigationBase>[] childItems;

		public NavigationRootView(INavigator navigator, NavigationFactory factory)
			: base(LocationKey.Root, navigator)
		{
			Render = RenderHint.Short;
			Reorderable = true;
			IsEditable = false;
			this.factory = factory;
			RootMenuHelper.ListChanged += HandleMainMenuChanged;
		}

		public override void Localize()
		{
			Name = Labels.NavigationRoot;
		}

		private void HandleMainMenuChanged(object sender, EventArgs e)
		{
			ReleaseChildren();
			Children = GetChildren();
		}

		private void ReleaseChildren()
		{
			if (childItems == null) return;
			foreach (var child in childItems.Select(n => n.Value))
			{
				child.PropertyChanged -= HandleChildrenChanged;
				factory.Release(child);
			}

			childItems = null;
		}

		protected override void Dispose(bool disposing)
		{
			RootMenuHelper.ListChanged -= HandleMainMenuChanged;
			ReleaseChildren();
			base.Dispose(disposing);
		}

		public override void Reorder(NavigationBase child, int position)
		{
			RootMenuHelper.Move(child.Key, position);
		}

		protected override LocationKey[] GetChildren()
		{
			if (childItems == null)
			{
				childItems = RootMenuHelper.GetMenuItems()
				.Select(n => new KeyValuePair<LocationKey, NavigationBase>(n, factory.Get(n)))
				.Where(n => n.Value != null)
				.ToArray();

				foreach (var child in childItems.Select(n => n.Value))
				{
					child.PropertyChanged += HandleChildrenChanged;
				}
			}

			return childItems.Where(x => x.Value.Children.Any()).Select(x => x.Key).ToArray();
		}

		protected void HandleChildrenChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Children") return;
			Children = GetChildren();
		}
	}
}