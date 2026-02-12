using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ApplicationAV = Avalonia.Application;
using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
//using Avalonia.Threading;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Linq; // No longer needed ?

namespace ActivityRecorderClientAV
{
	public partial class NotificationFormAV : Window
	{
		//private static SolidColorBrush? bgcolor;
		//private static SolidColorBrush? textcolor;
		private const int WindowWidth = 300;
		private const int WindowHeight = 100;
		private static string cornerPosition = "";

#if WINDOWS
		const int margin = -1000; // This method might need refinement
#elif MACOS
		const int margin = 0;
#endif

		public NotificationFormAV() : this(string.Empty) // Calls the constructor with an empty message
		{
		}

		public NotificationFormAV(string message)
		{
			// Initialize UI and bind data
			InitializeComponent();
			ApplyThemeBackground();

			ThemeHelperAV.RegisterThemeChangeHandler(this);
			UpdateThemeIcons();
			ScaleHelperAV.ScaleChanged += UpdateWindowScaling;

			var scaleTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale);
			NotificationWindowLayoutTransformControl.LayoutTransform = scaleTransform;
			this.Width = WindowWidth * ScaleHelperAV.GlobalWindowScale;
			this.Height = WindowHeight * ScaleHelperAV.GlobalWindowScale;
			this.Topmost = true; // Always on top

			//Message = message; // Assign the message
			//DataContext = this;
			SetMessage(message);
			//Debug.WriteLine($"MessageText is {(MessageText == null ? "not initialized" : "initialized")}.");

			App.OpenNotificationWindows.Add(this);
			
			//this.DataContext = this;
			//this.Opened += (sender, e) => UpdateThemeIcons();
			
			ApplicationAV.Current!.PropertyChanged += (sender, e) =>
			{
				if (e.Property.Name == nameof(ApplicationAV.ActualThemeVariant)) UpdateThemeIcons();
			};

			//this.DataContext = new NotificationViewModel { Message = message };
		}

		/*
		private void InitializeComponent()	// THIS SHOULD NEVER BE USED, IT BREAKS AVALONIA'S OWN InitializeComponent() method!
		{
			AvaloniaXamlLoader.Load(this);
		}
		*/

		private void ApplyThemeBackground()
		{
			var currentTheme = ApplicationAV.Current?.ActualThemeVariant;

			if (currentTheme == Avalonia.Styling.ThemeVariant.Light)
			{
				this.Classes.Add("light");
				this.Classes.Remove("dark");
			}
			else
			{
				this.Classes.Add("dark");
				this.Classes.Remove("light");
			}
		}

		public void UpdateWindowScaling()
		{
			//Hide();
			SetWindowSize(ScaleHelperAV.GlobalWindowScale);
			//Show();
        }

		private void SetWindowSize(double scaleValue)
		{
			//bool ShowItAgain = false;

			var scaleTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale);
			NotificationWindowLayoutTransformControl.LayoutTransform = scaleTransform;

			//Hide();

			SetMaxWindowSize(scaleValue);
			ReleaseMaxWindowSize();
			
			//ShowNotificationAtCorner(this, cornerPosition);
			//Show();
		}

		private void SetMaxWindowSize(double scaleValue)
		{
			double newWidth = Math.Round(WindowWidth * scaleValue, 1);
			double newHeight = Math.Round(WindowHeight * scaleValue, 1);

			this.MaxWidth = newWidth;
			this.MaxHeight = newHeight;
			
			this.Width = newWidth;
			this.Height = newHeight;

			this.MinWidth = newWidth;
			this.MinHeight = newHeight;
		}

		private void ReleaseMaxWindowSize()
		{
			this.MaxWidth = double.PositiveInfinity;
			this.MaxHeight = double.PositiveInfinity;
		}

		private void TextBlock_PointerPressed(object? sender, PointerPressedEventArgs e)
		{
			// Open the Messages Window
			OpenMessagesWindow();
			// Perhaps :
			// AvaloniaApp.OpenMessagesWindow();
		}

		private void OpenMessagesWindow()
		{
			// Open the Messages Window
			//var messagesWindow = new MessagesWindow();
			//messagesWindow.Show();
			Debug.WriteLine("MessagesWindow");
		}

		private void OnCloseClick(object? sender, RoutedEventArgs e)
		{
			App.OpenNotificationWindows.Remove(this);
			// Close the notification window
			this.Close();
		}

		public void SetMessage(string message)
		{
			Debug.WriteLine(message);
			// Check if MessageText is initialized before setting the text
			if (MessageText != null)
			{
				MessageText.Text = message;
			}
			else
			{
				Debug.WriteLine("MessageText is not initialized.");
			}
		}

		public static void ShowNotification(string message, string corner)
		{
			cornerPosition = corner;
			var notificationWindow = new NotificationFormAV(message); // Initialize with message
			ShowNotificationAtCorner(notificationWindow, corner);
			//SetMessage(message);
		}

		private static void ShowNotificationAtCorner(Window notificationWindow, string corner)
		{
				//var screen = callerWindow.Screens.Primary; // Assuming 'callerWindow' is a Window object
				//if (screen is not null)
				if (App.PrimaryScreen is not null)
				{
					var screenBounds = App.PrimaryScreen.Bounds;
					int x = 0, y = 0;

					switch (corner.ToLower())
					{
						case "top-left":
							x = 0;
							y = margin;
							break;
						case "top-right":
							x = (int)(screenBounds.Width - notificationWindow.Width);
							y = margin;
							break;
						case "bottom-left":
							x = 0;
							y = (int)(screenBounds.Height - notificationWindow.Height - margin);
							break;
						case "bottom-right":
							x = (int)(screenBounds.Width - notificationWindow.Width);
							y = (int)(screenBounds.Height - notificationWindow.Height - margin);
							break;
					}

					Debug.WriteLine(corner);
					Debug.WriteLine("x = " + x);
					Debug.WriteLine("y = " + y);

					notificationWindow.Position = new PixelPoint(x, y);
					notificationWindow.Show();
					//notificationWindow.UpdateThemeIcons();
				}
		}

		private void UpdateThemeIcons()
		{
			AppResourcesAV.SetIcon(this, "SvgCloseIcon", "CloseIcon", AppResourcesAV.IsLightTheme());
		}
		
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			ScaleHelperAV.ScaleChanged -= UpdateWindowScaling;
		}

		/*
		// Define the NotificationViewModel class inside the same file
		public class NotificationViewModel
		{
			public required string Message { get; set; }
		}
		*/
	}
}
