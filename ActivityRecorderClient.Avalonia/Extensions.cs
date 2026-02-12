using Avalonia.Controls;
using Avalonia.Threading;
using MsBox.Avalonia.Base;
using System.Threading;

namespace Tct.ActivityRecorderClient.Avalonia
{
	public static class Extensions
	{
		public static TResult ShowWindow<TResult>(this IMsBox<TResult> msgBox)
		{
			var mainLoopSource = new CancellationTokenSource();
			var resultTask = Dispatcher.UIThread.InvokeAsync(async () =>
			{
				var result = await msgBox.ShowWindowAsync();
				mainLoopSource.Cancel();
				return result;
			});
			Dispatcher.UIThread.MainLoop(mainLoopSource.Token);
			return resultTask.Result;
		}

		public static TResult ShowWindowDialog<TResult>(this IMsBox<TResult> msgBox, Window owner)
		{
			var mainLoopSource = new CancellationTokenSource();
			var resultTask = Dispatcher.UIThread.InvokeAsync(async () =>
			{
				var result = await msgBox.ShowWindowDialogAsync(owner);
				mainLoopSource.Cancel();
				return result;
			});
			Dispatcher.UIThread.MainLoop(mainLoopSource.Token);
			return resultTask.Result;
		}
	}
}
