using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Meeting.CountDown
{
	public class MeetingCountDownWinService : MeetingCountDownService
	{
		private Form Owner { get; set; }

		CountDownForm currentCountDownForm;

		public MeetingCountDownWinService(INotificationService notificationService, CurrentWorkController currentWorkController, Form owner)
			: base(notificationService, currentWorkController)
		{
			Owner = owner;
		}

		protected override void StartWorkGui(WorkDataWithParentNames workDataWithParents, ManualWorkItem itemToCreate, Func<TimeSpan> getElapsedTime, bool isCountUp, Action<bool> onGuiClosed)
		{
			Debug.Assert(currentCountDownForm == null);
			var cdForm = new CountDownForm()
			{
				Owner = Owner,
				Title = string.Format(Labels.NotificationManualWorkCreatedTitle, workDataWithParents.WorkData.Name),
				Description = string.Format(Labels.NotificationManualWorkCreatedBody,
					TimeZone.CurrentTimeZone.ToLocalTime(itemToCreate.StartDate).TimeOfDay.ToHourMinuteSecondString(),
					TimeZone.CurrentTimeZone.ToLocalTime(itemToCreate.EndDate).TimeOfDay.ToHourMinuteSecondString(),
					workDataWithParents.FullName
					),
				TotalTime = itemToCreate.EndDate - itemToCreate.StartDate,
				GetElapsedTime = getElapsedTime,
				IsCountUp = isCountUp,
				OnUpdating = CreateWorkItem,
				OnFinishing = DeleteUnfinishedTimedTask
			};
			cdForm.FormClosed += (sender, _) =>
											{
												var from = (CountDownForm)sender;
												onGuiClosed(from.IsUserCloseNotForced);
												currentCountDownForm = null;
											};
			currentCountDownForm = cdForm;
			cdForm.Show();
		}

		protected override void StopWorkGui(bool isForced)
		{
			if (currentCountDownForm == null) return;
			currentCountDownForm.CloseFrom(isForced);
		}
	}
}
