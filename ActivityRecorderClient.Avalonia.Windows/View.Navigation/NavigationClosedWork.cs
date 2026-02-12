using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationClosedWork : NavigationBase, INavigationWithWork
	{
		public WorkDataWithParentNames Work { get; private set; }

		public NavigationClosedWork(INavigator navigator, WorkDataWithParentNames work)
			: base(LocationKey.CreateClosed(work), navigator)
		{
			Debug.Assert(work.WorkData.Id != null);
			Children = new LocationKey[0];
			isFavorite = false;
			IsWork = true;
			CanFavorite = false;
			Work = work;
			Name = work.WorkData.Name;
			Path = work.ParentNames;
			IsEditable = false;
			UpdateUsedTime();
		}

		public override void Localize()
		{
		}

		protected override LocationKey[] GetChildren()
		{
			return null;
		}

		private void UpdateUsedTime()
		{
			SimpleWorkTimeStats simpleStats = MenuQuery.Instance.SimpleWorkTimeStats.Value;
			if (simpleStats == null) return;
			var proc = MenuBuilderHelper.GetWorkStatForId(simpleStats, Id);
			UsedTime = proc.TotalWorkTime;
		}
	}
}
