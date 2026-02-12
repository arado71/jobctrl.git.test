using System;
using System.Diagnostics;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Update;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationWork : NavigationBase, INavigationWithWork
	{
		private static readonly Timer t = new Timer { Interval = 60000, Enabled = true };

		public WorkDataWithParentNames Work { get; private set; }

		public NavigationWork(INavigator navigator, WorkDataWithParentNames work)
			: base(LocationKey.CreateFrom(work.WorkData), navigator)
		{
			Debug.Assert(work.WorkData.Id != null);
			SetWorkData(work);
			Children = new LocationKey[0];
			isFavorite = FavoritesHelper.IsFavorite(Id);
			IsWork = true;
			CanFavorite = true;
			t.Tick += HandleTimerTicked;
			MenuQuery.Instance.SimpleWorkTimeStats.Changed += HandleWorkTimeStatsUpdated;
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleMenuUpdated;
			HandleTimerTicked(this, EventArgs.Empty);
			HandleWorkTimeStatsUpdated(this, EventArgs.Empty);
		}

		public override void Localize()
		{
		}

		protected override void Dispose(bool disposing)
		{
			t.Tick -= HandleTimerTicked;
			MenuQuery.Instance.SimpleWorkTimeStats.Changed -= HandleWorkTimeStatsUpdated;
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleMenuUpdated;
			base.Dispose(disposing);
		}

		protected override LocationKey[] GetChildren()
		{
			return null;
		}

		private void HandleMenuUpdated(object sender, EventArgs e)
		{
			var menuLookup = MenuQuery.Instance.ClientMenuLookup.Value;
			if (menuLookup == null) return;
			WorkDataWithParentNames workData;
			if (!menuLookup.WorkDataById.TryGetValue(Id, out workData))
			{
				return;
			}

			SetWorkData(workData);
		}

		private void HandleTimerTicked(object sender, EventArgs e)
		{
			UpdateRemainingTime();
		}

		private void HandleWorkTimeStatsUpdated(object sender, EventArgs e)
		{
			UpdateUsedTime();
		}

		private void SetWorkData(WorkDataWithParentNames workData)
		{
			Debug.Assert(workData != null);
			Work = workData;
			Name = workData.WorkData.Name;
			Path = workData.ParentNames;
			Priority = workData.WorkData.Priority;
			EndDate = workData.WorkData.EndDate;
			StartDate = workData.WorkData.StartDate;
			TotalTime = workData.WorkData.TargetTotalWorkTime;
			IsEditable = !workData.WorkData.IsDefault && !workData.WorkData.IsReadOnly;
			UpdateRemainingTime();
			UpdateUsedTime();
		}

		private void UpdateRemainingTime()
		{
			RemainingTime = Work.WorkData.EndDate != null ? (TimeSpan?)(Work.WorkData.EndDate.Value.Date.AddDays(1) - DateTime.Now) : null;
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