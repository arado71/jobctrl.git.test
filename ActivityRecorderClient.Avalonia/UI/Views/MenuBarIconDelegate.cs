#if MACOS
using System;
using System.Runtime.InteropServices;
//using Microsoft.Extensions.DependencyInjection;
using Avalonia.Threading;
using Foundation;
//using System.Diagnostics;

namespace ActivityRecorderClientAV
{
    public partial class MenuBarIconDelegateAV : NSObject
    {
        // Import the function from the Swift code to set the delegate
        [LibraryImport("libmenubar")]
        private static partial void setMenuBarIconDelegate(IntPtr delegateHandle);
        //private MainWindow? _mainWindow;

        // Constructor
        public MenuBarIconDelegateAV()
        {
            try
            {
                // Set this instance as the delegate
                setMenuBarIconDelegate(this.Handle);

                // Get the MainWindow instance via dependency injection
                // _mainWindow = App.ServiceProvider!.GetService<MainWindow>();
            }
            catch (DllNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationFormAV.ShowNotification("DllNotFoundException", "top-right");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                NotificationFormAV.ShowNotification(ex.Message, "top-right");
            }
        }

        // The callback function that will be invoked from Swift for LEFT CLICK
        [Export("menuBarIconLeftClicked")]
        public void MenuBarIconLeftClicked()
        {
            //Console.WriteLine("MACOS MENUBAR LEFT CLICK!");
            //ShowMainWindow();
        }

        // The callback function that will be invoked from Swift for RIGHT CLICK
        [Export("menuBarIconRightClicked")]
        public void MenuBarIconRightClicked()
        {
            //Console.WriteLine("MACOS MENUBAR RIGHT CLICK!");
            // Handle right-click action (like opening context menu)
            ShowMainWindow();
        }

        // The callback function that will be invoked from Swift for DOUBLE CLICK
        [Export("menuBarIconDoubleClicked")]
        public void MenuBarIconDoubleClicked()
        {
            // Console.WriteLine("MACOS MENUBAR DOUBLE CLICK!");
            // Handle double-click action (like toggling work state)

            MacOSHelperAV.ToggleState();

            // Check if the main window is created already before chaning workstate icon
            AvaloniaApp.MainWindowInstance?.SetWorkstateIcon();
            
        }

        // Method to bring up and focus the main window
        private static void ShowMainWindow()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                //var mainWindow = App.ServiceProvider!.GetService<MainWindow>();
                if (AvaloniaApp.MainWindowInstance != null)
                {
                    if (!AvaloniaApp.MainWindowInstance.IsVisible)
                    {
#if DEBUG
                        //Console.WriteLine("MainWindow found, showing and activating it");
#endif
                        MacOSHelperAV.PositionMainWindow(AvaloniaApp.MainWindowInstance);
                        AvaloniaApp.MainWindowInstance.Show();
                        AvaloniaApp.MainWindowInstance.Activate();
                    
                    }
                    else
                    {
                        AvaloniaApp.MainWindowInstance.Activate();
                    }
                }
                else
                {
#if DEBUG
                    Console.WriteLine("mainWindow is NULL!");
#endif
                }
            });
        }
    }
}
#endif