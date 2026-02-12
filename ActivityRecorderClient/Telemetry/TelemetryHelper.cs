using System;
using System.Diagnostics;
using Tct.ActivityRecorderClient.Telemetry.Data;

namespace Tct.ActivityRecorderClient.Telemetry
{
	public static class TelemetryHelper
	{
		// Events
		public const string KeyJcStarted = "JcStarted";
		public const string KeyJcLoggedIn = "JcLoggedIn";
		public const string KeyFeature = "Feature";
		public const string KeyStartAfterLogin = "StartAfterLogin";
		// Measurements
		public const string KeyException = "Exception";
		public const string KeyWorkGridFolder = "WorkGridFolder";
		// Observations
		public const string KeyOsVersion = "OsVersion";
		public const string KeyNetVersion = "NetVersion";
		public const string KeyJcVersion = "JcVersion";
		public const string KeyCollectedItem = "CollectedItem";
		public const string KeyFrozenPlugin = "FrozenPlugin";
		// Timers
		public const string KeyRuleEvaluationTimer = "RuleEvalTimer";
		public const string KeyCaptureTimer = "CaptureTimer";

		public static readonly string[] defaultKeys =
		{
			KeyException,
			KeyOsVersion,
			KeyNetVersion,
			KeyJcVersion,
			KeyJcStarted,
			KeyFeature,
			KeyWorkGridFolder,
			KeyStartAfterLogin,
		};

		public static string GetDefaultKeys()
		{
			return string.Join(",", defaultKeys);
		}

		public static void Measure(string eventName)
		{
			TelemetryCoordinator.Instance.RecordEvent(eventName);
		}

		public static void Measure<T>(string eventName, T value)
		{
			TelemetryCoordinator.Instance.RecordMeasurement(eventName, value);
		}

		public static void Observe<T>(string eventName, T value)
		{
			TelemetryCoordinator.Instance.RecordObservation(eventName, value);
		}

		public static void RecordFeature(string featureName, string action)
		{
			if (IsEnabled(KeyFeature))
			{
				Measure(KeyFeature, new FeatureData() { Name = featureName, Action = action });
			}
		}

		public static IDisposable MeasureElapsed(string eventName)
		{
			return IsEnabled(eventName) ? new TimeMeasurer(eventName) : null;
		}

		public static bool IsEnabled(string eventName)
		{
			return TelemetryCoordinator.Instance.IsEnabled(eventName);
		}

		public static void Save()
		{
			TelemetryCoordinator.Instance.Save();
		}

		private class TimeMeasurer : IDisposable
		{
			private readonly string eventName;
			private readonly Stopwatch stopwatch = Stopwatch.StartNew();

			public TimeMeasurer(string eventName)
			{
				this.eventName = eventName;
			}

			public void Dispose()
			{
				Measure(eventName, stopwatch.Elapsed.TotalMilliseconds);
			}
		}
	}
}
