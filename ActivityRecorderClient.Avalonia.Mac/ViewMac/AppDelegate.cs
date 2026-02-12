using ActivityRecorderClientAV;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using log4net;

namespace Tct.ActivityRecorderClient.ViewMac;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
	private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	public AppDelegate()
	{
	}

	ClassicDesktopStyleApplicationLifetime? _lifetime;
	public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
		.UsePlatformDetect()
		.WithInterFont()
		.LogToDelegate(msg => log.Warn(msg));

	public override void DidFinishLaunching(NSNotification notification)
	{
		var builder = BuildAvaloniaApp();
		_lifetime = new ClassicDesktopStyleApplicationLifetime();
		builder.SetupWithLifetime(_lifetime);
	    _lifetime.Start(Environment.GetCommandLineArgs());
	}


	public override void WillTerminate(NSNotification notification)
	{
		log.Info("Exit");
	}


}