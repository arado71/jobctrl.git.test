using log4net;
using System.ComponentModel;
using System.Diagnostics;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Extra;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.ViewMac;

[Register("AppDelegateTest")]
public class AppDelegateTest : NSApplicationDelegate
{
	private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	private readonly SynchronizationContext context;
	private readonly CurrentWorkController currentWorkController;
	private readonly CaptureCoordinator captureCoordinator;

	NSStatusItem statusItem;
	StatusBarMenuWindowController windowController;

	NSMenuItem miExit;
	NSMenuItem miCurrentWork;
	MenuMacBuilder menuBuilder;

	public AppDelegateTest()
	{
		((PlatformMac.PlatformFactory)Platform.Factory).GuiSynchronizationContext = new NSRunLoopSynchronizationContext();
		context = Platform.Factory.GetGuiSynchronizationContext();
		captureCoordinator = new CaptureCoordinator(
						context,
						Platform.Factory.GetNotificationService(),
						CurrentWorkControllerPropertyChanged,
						new ClientSettingsManager(),
						ApplicationStartType.Normal
					);
		currentWorkController = captureCoordinator.CurrentWorkController;
		//timeManager = new TimeManager(captureCoordinator.SystemEventsService);

		//captureCoordinator.WorkItemCreated += CaptureCoordinatorWorkItemCreated;
		//captureCoordinator.WorkItemManager.ConnectionStatusChanged += WorkItemManagerConnectionStatusChanged;
		//captureCoordinator.WorkItemManager.CannotPersistAndSendWorkItem += WorkItemManagerCannotPersistAndSendWorkItem;
		captureCoordinator.CurrentMenuChanged += MenuManagerCurrentMenuChanged;
		captureCoordinator.Start();
	}

	public override void DidFinishLaunching(NSNotification notification)
	{
		// Insert code here to initialize your application
		statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);
		statusItem.Button.Title = "😊";
		statusItem.Button.Activated += (sender, e) =>
		{
			if (windowController == null || windowController.Window.IsVisible == false)
			{
				ShowUI((NSObject)sender);
			}
			else
			{
				HideUI();
			}
		};


		miExit = new NSMenuItem(Labels.Menu_Exit, miExit_Click);
		miCurrentWork = new NSMenuItem(Labels.Menu_NoWorkToContinue, CurrentWorkClick);

		var menu = new NSMenu();
		menuBuilder = new MenuMacBuilder(menu);
		menuBuilder.MenuClick += MenuBuilderMenuClick;

		var niTaskBar = statusItem;
		niTaskBar.Menu = menu;
		niTaskBar.Menu.AddItem(miCurrentWork);
		niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
		//niTaskBar.Menu.AddItem(miTodaysWorkTime);
		niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
		//niTaskBar.Menu.AddItem(miWorkTimeFromSrv);
		niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
		niTaskBar.Menu.AddItem(menuBuilder.PlaceHolder);
		niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
		//niTaskBar.Menu.AddItem(miRecentWorks);
		niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
		//niTaskBar.Menu.AddItem(miLastUrl);
		//niTaskBar.Menu.AddItem(miSettings);
		niTaskBar.Menu.AddItem(miExit);

		menuBuilder.UpdateMenu(captureCoordinator.CurrentMenuLookup.ClientMenu);
		log.Debug("menuBuilder Menu Updated");
	}

	private void MenuBuilderMenuClick(object sender, WorkDataEventArgs e)
	{
		Debug.Assert(e.WorkData.Id.HasValue);
		if (e.WorkData.ManualAddWorkDuration.HasValue)
		{
			//todo not supported atm.
			return;
		}
		if (!CanStartWorkOrWarn()) return;
		currentWorkController.UserStartWork(e.WorkData);
	}

	private bool CanStartWorkOrWarn()
	{
		if (currentWorkController.CurrentWorkState == WorkState.NotWorking && false/*&& timeManager.IsTimeInvalid*/)
		{
			log.Info("Trying to start work but client time is invalid");
			/*notificationService.ShowNotification(nfInvalidTimeCannotWorkKey, nfInvalidTimeCannotWorkDuration,
												 Labels.NotificationCannotStartUserSelectedWorkInvalidTimeTitle, Labels.NotificationCannotStartUserSelectedWorkInvalidTimeBody, CurrentWorkController.NotWorkingColor);
			*/
			return false;
		}
		return true;
	}

	private void miExit_Click(object sender, EventArgs e)
	{
		/*if (currentWorkController.CurrentWorkState != WorkState.NotWorking) //we also need to confirm when not working temp
		{
			var result = notificationService.ShowMessageBox(
				Labels.ConfirmExitStillWorkingBody,
				Labels.ConfirmExitStillWorkingTitle,
				MessageBoxButtons.OKCancel
			);
			if (result != DialogResult.OK)
				return;
			log.Info("Exit confirmed while working");
		}
		else
		{
			log.Info("Exit clicked and not working");
		}*/
		NSApplication.SharedApplication.Terminate(this);
	}

	private void CurrentWorkControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != "CurrentWork")
			return;
		var cwc = (CurrentWorkController)sender;
		//display current workitem in menu (dis/enable)

		bool isWorking = cwc.CurrentWorkState == WorkState.Working || cwc.CurrentWorkState == WorkState.WorkingTemp;
		miCurrentWork.Title = (isWorking ? '\u25FC' : '\u25BA') + " " + cwc.CurrentOrLastWorkName;

		miCurrentWork.Enabled = isWorking;
		//display current workitem in tooltip and recalculate worktimes
		//UpdateWorkTimeAndTaskbarInfo(isWorking ? cwc.CurrentOrLastWorkNameInTwoLines : null);
		//display icon
		//todo different icon for NotWorkingTemp
		//niTaskBar.Image = isWorking ? workingImage : notWorkingImage;
		statusItem.Title = isWorking ? "😁" : "😴";
		//misc.
		/*if (isWorking)
		{
			notificationService.HideNotification(nfIdleStopWorkKey);
			if (cwc.IsCurrentWorkValid)
				miRecentWorks.AddRecentWork(cwc.CurrentWork);
		}
		else
		{
			idleDetector.ResetIdleWorkTime();
		}*/
	}

	private void CurrentWorkClick(object sender, EventArgs e)
	{
		if (currentWorkController.CurrentWork == null)
		{
			if (!CanStartWorkOrWarn()) return;
			currentWorkController.UserResumeWork();
		}
		else
		{
			currentWorkController.UserStopWork();
		}
	}

	private void MenuManagerCurrentMenuChanged(object sender, MenuEventArgs e)
	{
		menuBuilder.UpdateMenu(e.Menu);
		//miRecentWorks.UpdateMenu(e.Menu);
	}

	public override void WillTerminate(NSNotification notification)
	{
		// Insert code here to tear down your application
		log.Info("Exit");
	}

	private void HideUI()
	{
		windowController.Close();
	}

	private void ShowUI(NSObject sender)
	{
		if (windowController == null)
		{
			windowController = new StatusBarMenuWindowController(statusItem, new DummyContentViewController());
		}

		windowController.ShowWindow(sender);
	}
}

public class DummyContentViewController : NSViewController
{
	public DummyContentViewController() : base()
	{
		PreferredContentSize = new CGSize(290, 300);
	}

	public override void LoadView()
	{
		View = new NSView
		{
			WantsLayer = true,
			Layer = { BackgroundColor = NSColor.WindowBackground.CGColor }
		};
	}
}

public class StatusBarMenuWindowController : NSWindowController, INSWindowDelegate
{
	private NSStatusItem statusItem;
	private NSViewController contentViewController;
	private EventMonitor eventMonitor;

	public StatusBarMenuWindowController(NSStatusItem statusItem, NSViewController contentViewController)
	{
		this.statusItem = statusItem;
		this.contentViewController = contentViewController;

		this.Window = new NSWindow(
			new CoreGraphics.CGRect(0, 0, 344, 320),
			NSWindowStyle.FullSizeContentView | NSWindowStyle.Titled,
			NSBackingStore.Buffered,
			false,
			screen: statusItem!.Button!.Window!.Screen
		);

		this.Window.MovableByWindowBackground = false;
		this.Window.TitleVisibility = NSWindowTitleVisibility.Hidden;
		this.Window.TitlebarAppearsTransparent = true;
		this.Window.Level = NSWindowLevel.Status;
		this.Window.ContentViewController = contentViewController;

		if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
		{
			this.Window.IsOpaque = false;
			this.Window.BackgroundColor = NSColor.Clear;
		}

		this.Window.Delegate = this;
		this.RepositionWindow();
	}

	public override void ShowWindow(NSObject sender)
	{
		PostBeginMenuTrackingNotification();
		if (OperatingSystem.IsOSPlatformVersionAtLeast("macOS", 14))
		{
			NSApplication.SharedApplication.Activate();
		}
		else
		{
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
		}
		RepositionWindow();
		Window.AlphaValue = 1;
		base.ShowWindow(sender);
		StartMonitoringClicks();
	}

	public override void Close()
	{
		PostEndMenuTrackingNotification();
		NSAnimationContext.BeginGrouping();
		NSAnimationContext.CurrentContext.CompletionHandler = () =>
		{
			base.Close();
			eventMonitor?.Stop();
			eventMonitor = null;
		};
		//Window.Animator.AlphaValue = 0;
		NSAnimationContext.EndGrouping();
	}

	private void StartMonitoringClicks()
	{
		eventMonitor = new EventMonitor(NSEventMask.LeftMouseDown | NSEventMask.RightMouseDown, (NSEvent theEvent) =>
		{
			Close();
		});
		eventMonitor.Start();
	}

	private void RepositionWindow()
	{
		var referenceWindow = statusItem?.Button?.Window;
		if (referenceWindow == null || this.Window == null)
		{
			Console.WriteLine("Couldn't find reference window for repositioning status bar menu window, centering instead");
			this.Window?.Center();
			return;
		}

		var width = contentViewController?.PreferredContentSize.Width ?? this.Window.Frame.Width;
		var height = contentViewController?.PreferredContentSize.Height ?? this.Window.Frame.Height;
		var x = referenceWindow.Frame.X + (referenceWindow.Frame.Width / 2) - (this.Window.Frame.Width / 2);

		if (referenceWindow.Screen != null)
		{
			var screen = referenceWindow.Screen;
			// If the window extrapolates the limits of the screen, reposition it.
			if ((x + width) > (screen.VisibleFrame.X + screen.VisibleFrame.Width))
			{
				x = (screen.VisibleFrame.X + screen.VisibleFrame.Width) - width - Metrics.Margin;
			}
		}

		var rect = new CGRect(
			x: x,
			y: referenceWindow.Frame.Y - height - Metrics.Margin,
			width: width,
			height: height
		);

		this.Window.SetFrame(rect, display: true, animate: false);
	}

	private struct Metrics
	{
		public static readonly nfloat Margin = 5;
	}

	// Implement other methods and properties as needed, including window delegate methods and content size observation

	private void PostBeginMenuTrackingNotification()
	{
		NSDistributedNotificationCenter.DefaultCenter.PostNotificationName("com.apple.HIToolbox.beginMenuTrackingNotification", null);
	}

	private void PostEndMenuTrackingNotification()
	{
		NSDistributedNotificationCenter.DefaultCenter.PostNotificationName("com.apple.HIToolbox.endMenuTrackingNotification", null);
	}

	// Window Delegate Methods
	[Export("windowWillClose:")]
	public void WindowWillClose(NSNotification notification)
	{
		// Your window will close logic here
	}

	[Export("windowDidBecomeKey:")]
	public void WindowDidBecomeKey(NSNotification notification)
	{
		statusItem.Button.Highlight(true);
	}

	[Export("windowDidResignKey:")]
	public void WindowDidResignKey(NSNotification notification)
	{
		statusItem.Button.Highlight(false);
	}
}

public class EventMonitor
{
	private NSObject monitor;
	private NSEventMask mask;
	private Action<NSEvent> handler;

	public EventMonitor(NSEventMask mask, Action<NSEvent> handler)
	{
		this.mask = mask;
		this.handler = handler;
	}

	~EventMonitor()
	{
		Stop();
	}

	public void Start()
	{
		monitor = NSEvent.AddGlobalMonitorForEventsMatchingMask(mask, (theEvent) => handler(theEvent));
	}

	public void Stop()
	{
		if (monitor != null)
		{
			NSEvent.RemoveMonitor(monitor);
			monitor = null;
		}
	}
}

