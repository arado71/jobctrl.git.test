using System;
using AppKit;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public class NSMenuItemWithWorkData : NSMenuItem
	{
		private float? barValue;

		public float? BarValue
		{
			get { return barValue;}
			set
			{
				if (barValue == value)
					return;
				barValue = value;
				if (Image != null)
					Image.Dispose();
				Image = ImageHelper.GetProgressBarImage(BarValue, BarText);
			}
		}

		private string barText;

		public string BarText
		{
			get{ return barText;}
			set
			{
				if (barText == value)
					return;
				barText = value;
				if (Image != null)
					Image.Dispose();
				Image = ImageHelper.GetProgressBarImage(BarValue, BarText);
			}
		}

		public WorkData WorkData { get; set; }

		public event EventHandler<WorkDataEventArgs> Click;

		public NSMenuItemWithWorkData(string title)
			: base (title, OnActivated)
		{
		}

		//avoid generating two images when one would be enough
		public void SetBarValueAndText(float? barValue, string barText)
		{
			if (this.barValue == barValue && this.barText == barText)
				return;
			this.barValue = barValue;
			this.barText = barText;
			if (Image != null)
				Image.Dispose();
			Image = ImageHelper.GetProgressBarImage(BarValue, BarText);
		}

		private static void OnActivated(object sender, EventArgs args)
		{
			((NSMenuItemWithWorkData)sender).OnClick();
		}

		private void OnClick()
		{
			var click = Click;
			if (click != null)
				click(this, new WorkDataEventArgs(WorkData));
		}

		public static string GetWorkDataDesc(WorkData workData)
		{
			if (workData == null)
				return "";
			return (workData.Id.HasValue ? "Id: " + workData.Id : (Labels.WorkData_Project + (workData.ProjectId.HasValue ? " Id: " + workData.ProjectId : "")))
				+ (workData.ExtId.HasValue ? " (" + workData.ExtId + ")" : "")
				+ (workData.Priority.HasValue ? Environment.NewLine + Labels.WorkData_Priority + ": " + workData.Priority : "")
				+ (workData.StartDate.HasValue ? Environment.NewLine + Labels.WorkData_StartDate + ": " + workData.StartDate.Value.ToShortDateString() : "")
				+ (workData.EndDate.HasValue ? Environment.NewLine + Labels.WorkData_EndDate + ": " + workData.EndDate.Value.ToShortDateString() : "")
				+ (workData.TargetTotalWorkTime.HasValue ? Environment.NewLine + Labels.WorkData_TargetHours + ": " + workData.TargetTotalWorkTime.Value.TotalHours.ToString("0.#") : "");
		}
	}
}

