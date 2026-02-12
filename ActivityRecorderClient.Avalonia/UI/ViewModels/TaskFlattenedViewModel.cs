using ActivityRecorderClient.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class TaskFlattenedViewModel : ViewModelBase
	{
		[ObservableProperty]
		private int workId;

		[ObservableProperty]
		private string name = "";

		[ObservableProperty]
		private string fullName = "";

		public static List<TaskFlattenedViewModel> CreateListFrom(ClientMenu menu, Func<WorkData, bool>? canSelectWork)
		{
			return MenuHelper.FlattenDistinctWorkDataThatHasId(menu)
				.Where(work => canSelectWork?.Invoke(work.WorkData) ?? true)
				.Select(work => new TaskFlattenedViewModel
				{
					WorkId = work.WorkData.Id!.Value,
					Name = work.WorkData.Name ?? "",
					FullName = work.FullName ?? "",
				})
				.ToList();
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
