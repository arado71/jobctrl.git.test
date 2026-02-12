using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ApplicationAV = Avalonia.Application;

namespace ActivityRecorderClientAV
{
	public partial class DebugWindowAV : Window
    {
        private static DebugWindowAV? _instance;

        public static DebugWindowAV Instance
        {
            get
            {
                // Lazily initialize the instance when needed
                _instance ??= new DebugWindowAV();
                return _instance;
            }
        }

        private const int WindowWidth = 600;
		private const int WindowHeight = 400;


        public DebugWindowAV()
        {
            InitializeComponent();
            AttachListeners();
            
            #if WINDOWS
			    InitTaskbarIcon();
			#endif
            
            InitScaleTransform();
            ApplyThemeBackground();
            ThemeHelperAV.RegisterThemeChangeHandler(this);
            ScaleHelperAV.ScaleChanged += UpdateWindowScaling;
        }

        private void InitTaskbarIcon()
		{
			try
			{   this.Icon = new WindowIcon("Assets/JobCtrl.ico"); }
			catch (Exception ex)
			{   Debug.WriteLine("Error initializing tray icon: " + ex.Message); }
		}

        public void ApplyThemeBackground()
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

        private void InitScaleTransform()
        {
            var scaleTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale);
			DebugWindowLayoutTransformControl.LayoutTransform = scaleTransform;
			this.Width = WindowWidth * ScaleHelperAV.GlobalWindowScale;
			//this.MinWidth = WindowWidth * ScaleHelper.GlobalWindowScale;
			this.Height = WindowHeight * ScaleHelperAV.GlobalWindowScale;
			//this.MinHeight = WindowHeight * ScaleHelper.GlobalWindowScale;
        }

        public void UpdateWindowScaling()
		{
            bool ShowItAgain = false;            

			var scaleTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale);
			if (DebugWindowLayoutTransformControl != null) DebugWindowLayoutTransformControl.LayoutTransform = scaleTransform;
			
			if (this.IsVisible)
			{ 
				ShowItAgain = true;
				Hide();
			}
			
			SetMaxWindowSize();
			ReleaseMaxWindowSize();

			if (ShowItAgain)
			{
				Show();
			}
        }

        private void SetMaxWindowSize()
		{
			double newWidth = Math.Round(WindowWidth * ScaleHelperAV.GlobalWindowScale, 1);
			double newHeight = Math.Round(WindowHeight * ScaleHelperAV.GlobalWindowScale, 1);

			this.MaxWidth = newWidth;
			this.MaxHeight = newHeight;
			
			this.Width = newWidth;
			this.Height = newHeight;

			//this.MinWidth = newWidth;
			//this.MinHeight = newHeight;
		}

        private void ReleaseMaxWindowSize()
		{
            this.MaxWidth = double.PositiveInfinity;
            this.MaxHeight = double.PositiveInfinity;
		}

        private void AttachListeners()
        {
            // Redirect Console.WriteLine
            Console.SetOut(new DebugTextWriter(message => AppendMessage(message)));

            // Redirect Debug.WriteLine (via Trace.Listeners)
            Trace.Listeners.Add(new DebugWindowListener(message => AppendMessage(message)));
        }

        public void AppendMessage(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                DebugOutput.Text += message + Environment.NewLine;
                DebugOutput.CaretIndex = DebugOutput.Text.Length; // Scroll to bottom
            });
        }

        // Override the Closing event to hide the window instead of closing it
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);

            // Cancel the actual closing process
            e.Cancel = true;

            // Hide the window
            Hide();

            // Clear debug messages
            DebugOutput.Text = string.Empty;
        }

        private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			// Check if the left mouse button is pressed
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				BeginMoveDrag(e); // Initiates the drag operation
			}
		}
    }

    // Custom TextWriter for Console
    public class DebugTextWriter : TextWriter
    {
        private readonly Action<string> _writeAction;

        public DebugTextWriter(Action<string> writeAction)
        {
            _writeAction = writeAction;
        }

        public override void WriteLine(string? value)
        {
            _writeAction(value ?? string.Empty);
        }

        public override void Write(char value)
        {
            _writeAction(value.ToString());
        }

        public override Encoding Encoding => Encoding.UTF8;
    }

    // Custom TraceListener for Debug
    public class DebugWindowListener : TraceListener
    {
        private readonly Action<string> _writeAction;

        public DebugWindowListener(Action<string> writeAction)
        {
            _writeAction = writeAction;
        }

        public override void Write(string? message)
        {
            if (!string.IsNullOrEmpty(message))
                _writeAction(message);
        }

        public override void WriteLine(string? message)
        {
            if (!string.IsNullOrEmpty(message))
                _writeAction(message);
        }
    }
}