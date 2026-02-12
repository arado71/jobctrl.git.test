using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient
{
	public static class FeatureSwitches
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static HashSet<string> features = new HashSet<string>();

		public static event EventHandler<SingleValueEventArgs<string>> FeatureEnabled; 
		public static event EventHandler<SingleValueEventArgs<string>> FeatureDisabled; 
		public static event EventHandler<SingleValueEventArgs<string>> FeatureChanged; 

		public static void UpdateFeatures(string enabledFeatures)
		{
			var newFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (!string.IsNullOrEmpty(enabledFeatures))
			{
				foreach (var newFeature in enabledFeatures.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					newFeatures.Add(newFeature.Trim());
				}
			}

			bool tryAgain;
			HashSet<string> oldFeatures;
			do
			{
				oldFeatures = Interlocked.CompareExchange(ref features, null, null);
				if (oldFeatures != null && oldFeatures.SetEquals(newFeatures)) return;
				tryAgain = Interlocked.CompareExchange(ref features, newFeatures, oldFeatures) != oldFeatures;
			} while (tryAgain);

			log.Info("Enabled features changed: " + enabledFeatures);
			IEnumerable<string> addedFeatures = newFeatures;
			IEnumerable<string> removedFeatures = Enumerable.Empty<string>();
			if (oldFeatures != null)
			{
				addedFeatures = newFeatures.Except(oldFeatures);
				removedFeatures = oldFeatures.Except(newFeatures);
			}

			foreach (var removedFeature in removedFeatures)
			{
				OnFeatureDisabled(removedFeature);
				OnFeatureChanged(removedFeature);
			}

			foreach (var addedFeature in addedFeatures)
			{
				OnFeatureEnabled(addedFeature);
				OnFeatureChanged(addedFeature);
			}
		}

		public static IEnumerable<string> GetEnabledFeatures()
		{
			var currentFeatures = Interlocked.CompareExchange(ref features, null, null);
			return currentFeatures ?? Enumerable.Empty<string>();
		}

		public static bool IsEnabled(string feature)
		{
			var currentFeatures = Interlocked.CompareExchange(ref features, null, null);
			return currentFeatures != null && currentFeatures.Contains(feature);
		}

		private static void OnFeatureEnabled(string featureName)
		{
			var evt = FeatureEnabled;
			if (evt != null) evt(null, new SingleValueEventArgs<string>(featureName));
		}

		private static void OnFeatureDisabled(string featureName)
		{
			var evt = FeatureDisabled;
			if (evt != null) evt(null, new SingleValueEventArgs<string>(featureName));
		}

		private static void OnFeatureChanged(string featureName)
		{
			var evt = FeatureChanged;
			if (evt != null) evt(null, new SingleValueEventArgs<string>(featureName));
		}
	}

	public static class Features
	{
		// Don't delete or reuse any feature constants (http://dougseven.com/2014/04/17/knightmare-a-devops-cautionary-tale/)

		public const string ForceCountdownRules = "ForceCountdownRules";
	}
}
