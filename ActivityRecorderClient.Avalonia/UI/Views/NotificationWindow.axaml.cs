using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Avalonia.UI.Views;

public partial class NotificationWindow : BaseWindow
{
	public NotificationViewModel? ViewModel
	{
		get => DataContext as NotificationViewModel;
		set
		{
			if (DataContext is NotificationViewModel oldContext)
			{
				oldContext.PropertyChanged -= OnPropertyChanged;
			}
			DataContext = value;
			if (value != null)
			{
				value.PropertyChanged += OnPropertyChanged;
				ApplyPosition(value.Position);
			}
		}
	}

	public NotificationWindow()
	{
		InitializeComponent();
	}

	private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(NotificationViewModel.Position) && sender is NotificationViewModel vm)
		{
			ApplyPosition(vm.Position);
		}
	}

	private void ApplyPosition(NotificationPosition position)
	{
		var screen = Screens.Primary;
		var windowWidth = Bounds.Width > 0 ? Bounds.Width : Width;
		var windowHeight = Bounds.Height > 0 ? Bounds.Height : Height;
		var screenWidth = screen.WorkingArea.Width;
		var screenHeight = screen.WorkingArea.Height;

		PixelPoint newPos = position switch
		{
			NotificationPosition.TopLeft => new PixelPoint(0, 0),
			NotificationPosition.TopRight => new PixelPoint(screenWidth - (int)windowWidth, 0),
			NotificationPosition.BottomLeft => new PixelPoint(0, screenHeight - (int)windowHeight),
			NotificationPosition.BottomRight => new PixelPoint(screenWidth - (int)windowWidth, screenHeight - (int)windowHeight),
			NotificationPosition.Center => new PixelPoint((screenWidth - (int)windowWidth) / 2, (screenHeight - (int)windowHeight) / 2),
			NotificationPosition.Hidden => new PixelPoint(-9999, -9999),
			_ => new PixelPoint(0, 0),
		};

		Position = newPos;
	}

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);

		if (DataContext is NotificationViewModel vm)
		{
			ApplyPosition(vm.Position);

			if (vm.ShowDuration > TimeSpan.Zero)
			{
				CloseAfter(vm.ShowDuration);
			}
		}
	}

	protected override void OnResized(WindowResizedEventArgs e)
	{
		base.OnResized(e);

		if (DataContext is NotificationViewModel vm)
		{
			ApplyPosition(vm.Position);
		}
	}

	private async void CloseAfter(TimeSpan delay)
	{
		await Task.Delay(delay);
		Close();
	}

	private void OnCloseClick(object sender, RoutedEventArgs e)
	{
		Close();
	}

	// TODO: mac, close animation
	public void CloseFast()
	{
		Close();
	}
}