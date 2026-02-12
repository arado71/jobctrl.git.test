using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System.Diagnostics;
using WindowAV = Avalonia.Controls.Window;
using KeyEventArgsAV = Avalonia.Input.KeyEventArgs;
using ApplicationAV = Avalonia.Application;

namespace ActivityRecorderClientAV
{
	public abstract class BaseWindowAV : WindowAV
	{
		protected abstract int WindowWidth { get; }
		protected abstract int WindowHeight { get; }
		protected virtual int? WindowMaxWidth => null;
		protected virtual int? WindowMaxHeight => null;
		
		protected abstract LayoutTransformControl? LayoutTransformController { get; }

		public BaseWindowAV()
		{
			KeyDown += OnKeyDown;
			Focusable = true;

			ThemeHelperAV.RegisterThemeChangeHandler(this);
			ScaleHelperAV.ScaleChanged += UpdateWindowScaling;

#if WINDOWS
			try
			{ this.Icon = new WindowIcon("Assets/JobCtrl.ico"); }
			catch (Exception ex)
			{ Debug.WriteLine("Error initializing tray icon: " + ex.Message); }
#endif
		}

		protected virtual void OnInitialize()
		{
			ApplyThemeBackground();
			InitWindowSize();
			InitWindowScaling();
		}

		private void ApplyThemeBackground()
		{
			var currentTheme = ApplicationAV.Current?.ActualThemeVariant;

			if (currentTheme == Avalonia.Styling.ThemeVariant.Light)
			{
				Classes.Add("light");
				Classes.Remove("dark");
			}
			else
			{
				Classes.Add("dark");
				Classes.Remove("light");
			}
		}

		public void UpdateWindowScaling()
		{
			bool ShowItAgain = false;
			PixelPoint? centerPos = GetWindowCenter(this);
			Debug.WriteLine("******centerPos = " + centerPos);

			var scaleTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale);
			if (LayoutTransformController != null) LayoutTransformController.LayoutTransform = scaleTransform;

			if (IsVisible)
			{
				ShowItAgain = true;
			}

			SetMaxWindowSize();
			ReleaseMaxWindowSize();

			if (ShowItAgain)
			{
				if (centerPos != null) SetWindowCenter(this, (PixelPoint)centerPos);
			}
		}

		public void InitWindowScaling()
		{
			Debug.WriteLine("BaseWindow InitWindowScaling");
			var scaleTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale);
			if (LayoutTransformController != null)
			{
				LayoutTransformController.LayoutTransform = scaleTransform;
				Debug.WriteLine("BaseWindow Setting Scale");
			}
			else Debug.WriteLine("BaseWindow Setting Scale failed");
		}

		public void InitWindowSize()
		{
			MinWidth = WindowWidth * ScaleHelperAV.GlobalWindowScale;
			MinHeight = WindowHeight * ScaleHelperAV.GlobalWindowScale;
			Width = WindowWidth * ScaleHelperAV.GlobalWindowScale;
			Height = WindowHeight * ScaleHelperAV.GlobalWindowScale;
			ReleaseMaxWindowSize();
		}

		private void SetMaxWindowSize()
		{
			double newWidth = Math.Round(WindowWidth * ScaleHelperAV.GlobalWindowScale, 1);
			double newHeight = Math.Round(WindowHeight * ScaleHelperAV.GlobalWindowScale, 1);

			MaxWidth = newWidth;
			MaxHeight = newHeight;

			Width = newWidth;
			Height = newHeight;

			MinWidth = newWidth;
			MinHeight = newHeight;
		}

		private void ReleaseMaxWindowSize()
		{
			if (WindowMaxWidth.HasValue)
			{
				MaxWidth = (double)(WindowMaxWidth * ScaleHelperAV.GlobalWindowScale);
			}
			else MaxWidth = double.PositiveInfinity;

			if (WindowMaxHeight.HasValue)
			{
				MaxHeight = (double)(WindowMaxHeight * ScaleHelperAV.GlobalWindowScale);
			}
			else MaxHeight = double.PositiveInfinity;
		}

		private PixelPoint? GetWindowCenter(WindowAV window)
		{
			var screens = window.Screens;
			if (screens == null) return null;
			//var screen = screens.ScreenFromPoint(new PixelPoint(0,0));
			var screen = screens.ScreenFromWindow(window);
			if (screen == null) return null;
			if (window.WindowState == WindowState.Minimized) return null; // Remove on macOS for testing

			var scalingFactor = screen.Scaling;

			var windowX = window.Position.X;
			var windowY = window.Position.Y;
			Debug.WriteLine("______windowX = " + windowX);
			Debug.WriteLine("______windowY = " + windowY);

			var windowWidth = window.Width * scalingFactor;
			var windowHeight = window.Height * scalingFactor;
			Debug.WriteLine("______windowWidth = " + windowWidth);
			Debug.WriteLine("______windowHeight = " + windowHeight);

			Debug.WriteLine("______WorkingArea.Width = " + screen.WorkingArea.Width);
			Debug.WriteLine("______WorkingArea.Height = " + screen.WorkingArea.Height);

			//var centerX = (screen.WorkingArea.Width - windowWidth) / 2;
			//var centerY = (screen.WorkingArea.Height - windowHeight) / 2;
			var centerX = windowX + windowWidth / 2;
			var centerY = windowY + windowHeight / 2;
			Debug.WriteLine("______centerX = " + centerX);
			Debug.WriteLine("______centerY = " + centerY);

			//if ((centerX < 0) || (centerY < 0)) return null; // Put back on macOS for testing

			return new PixelPoint((int)centerX, (int)centerY);
		}

		private void SetWindowCenter(WindowAV window, PixelPoint center)
		{
			var screens = window.Screens;
			if (screens == null) return;
			var screen = screens.ScreenFromWindow(window);
			if (screen == null) return;

			var scalingFactor = screen.Scaling;

			var windowWidth = window.Width * scalingFactor;
			var windowHeight = window.Height * scalingFactor;

			var newWindowX = center.X - windowWidth / 2;
			var newWindowY = center.Y - windowHeight / 2;

			newWindowX = Math.Max(newWindowX, 0); // Ensure X is not negative
			newWindowY = Math.Max(newWindowY, 0); // Ensure Y is not negative
			var screenWidth = screen.WorkingArea.Width;
			var screenHeight = screen.WorkingArea.Height;
			newWindowX = Math.Min(newWindowX, screenWidth - windowWidth); // Ensure the window does not go beyond the width
			newWindowY = Math.Min(newWindowY, screenHeight - windowHeight); // Ensure the window does not go beyond the height

			window.Position = new PixelPoint((int)newWindowX, (int)newWindowY);
		}

		private void OnKeyDown(object? sender, KeyEventArgsAV e)
		{
			if (e.Key == Key.W && (e.KeyModifiers & KeyModifiers.Meta) != 0 || // Cmd + W on macOS
				e.Key == Key.W && (e.KeyModifiers & KeyModifiers.Control) != 0) // Ctrl + W on Windows
			{ Close(); return; }
			// { Debug.WriteLine("CMD+W Detected!"); return; }

			// if (e.Key == Key.H && (e.KeyModifiers & KeyModifiers.Meta) != 0) // Cmd + H on macOS
			// { Hide(); return; }	// This works but seems useless/undesirable since window will disappear and won't be visible on dock either

			if (e.Key == Key.M && (e.KeyModifiers & KeyModifiers.Meta) != 0 || // Cmd + M on macOS
				e.Key == Key.M && (e.KeyModifiers & KeyModifiers.Control) != 0) // Ctrl + M on Windows
				WindowState = WindowState.Minimized;

			// Hiding on Windows makes no sense, Window will not reappear as on macOS
			// if (e.Key == Key.H && (e.KeyModifiers & KeyModifiers.Control) != 0) // Ctrl + H on Windows 
			// {Hide(); return;}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			ScaleHelperAV.ScaleChanged -= UpdateWindowScaling;
			KeyDown -= OnKeyDown;
		}
	}
}