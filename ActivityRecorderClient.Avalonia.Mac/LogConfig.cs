using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;

public static class LogConfig
{
	public static void ConfigureLogging()
	{
		var hierarchy = (Hierarchy)LogManager.GetRepository();

		var layout = new PatternLayout
		{
			ConversionPattern = "%date [%2thread] %-5level %24.24logger{1} - %message%newline"
		};
		layout.ActivateOptions();

		var filter = new LevelRangeFilter
		{
			LevelMin = Level.Info,
			LevelMax = Level.Fatal
		};
		filter.ActivateOptions();

		var appender = new RollingFileAppender
		{
			Name = "RollingLogFileAppender",
			File = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}/Library/Logs/JobCTRL/JobCTRL.log",
			AppendToFile = true,
			RollingStyle = RollingFileAppender.RollingMode.Size,
			MaximumFileSize = "10MB",
			MaxSizeRollBackups = 5,
			StaticLogFileName = true,
			Layout = layout
		};

		appender.AddFilter(filter);
		appender.ActivateOptions();

		hierarchy.Root.Level = Level.Debug;
		hierarchy.Root.AddAppender(appender);
		hierarchy.Configured = true;
	}
}
