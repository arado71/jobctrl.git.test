using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	public partial class WorkItem
	{
		private EntitySet<ScreenShot> _ScreenShots;

		internal EntitySet<ScreenShot> ScreenShotsInt
		{
			get { return this._ScreenShots; }
			set { this._ScreenShots.Assign(value); }
		}

		public EntitySet<ScreenShot> ScreenShots
		{
			get { return ScreenShotsInt; }
			set { ScreenShotsInt = value; }
		}

		partial void OnCreated()
		{
			_ScreenShots = new EntitySet<ScreenShot>(attach_ScreenShots, detach_ScreenShots);
		}

		// ReSharper disable UnusedMember.Local
		partial void OnValidate(ChangeAction action)
		{
			if (action != ChangeAction.Insert) return;
			foreach (var screenShot in ScreenShots)
			{
				screenShot.ReceiveDate = DateTime.UtcNow;
				screenShot.CreateDate = Clamp(screenShot.CreateDate, this.StartDate, this.EndDate);
				screenShot.UserId = UserId;
				screenShot.ComputerId = ComputerId;
			}
		}
		// ReSharper restore UnusedMember.Local

		private static DateTime Clamp(DateTime value, DateTime minValue, DateTime maxValue)
		{
			return value < minValue ? minValue : value > maxValue ? maxValue : value;
		}

		private void attach_ScreenShots(ScreenShot entity)
		{
			this.SendPropertyChanging();
			entity.WorkItem = this;
		}

		private void detach_ScreenShots(ScreenShot entity)
		{
			this.SendPropertyChanging();
			entity.WorkItem = null;
		}

		public override string ToString()
		{
			return "uid: " + UserId + " wid: " + WorkId + " start: " + StartDate + " end: " + EndDate;
		}

	}

	public partial class ActivityRecorderDataClassesDataContext
	{
		public override void SubmitChanges(System.Data.Linq.ConflictMode failureMode)
		{
			var changeSet = GetChangeSet();
			var insertedWorkItems = changeSet.Inserts.OfType<WorkItem>().ToArray();
			foreach (var workItem in insertedWorkItems)
			{
				ScreenShots.InsertAllOnSubmit(workItem.ScreenShots);
			}
			base.SubmitChanges(failureMode);
		}
	}
}
