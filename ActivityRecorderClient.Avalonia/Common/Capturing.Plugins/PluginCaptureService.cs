using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Telemetry;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public abstract class PluginCaptureService : IPluginCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ICaptureExtensionAdapter[] emptyExtensions = new ICaptureExtensionAdapter[0];

		private readonly object thisLock = new object();

		private int currentRuleVersion;
		private int numBgRefreshRunning;
		private int numInUseNotificationReq;
		private bool useAsyncCapturing;
		private readonly ParallelCollection<ICaptureExtensionAdapter> parallelCollection = new ParallelCollection<ICaptureExtensionAdapter>(null, TimeSpan.FromMilliseconds(ConfigManager.RuleMatchingInterval), x => "Async" + x.CaptureExtensionSettings.PluginId);
		private readonly CachedDictionary<string, Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]>> cachedPluginValues = new CachedDictionary<string, Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]>>(TimeSpan.FromMinutes(1), true);
		private readonly List<ICaptureExtensionAdapter[]> captureExtensionsInUse = new List<ICaptureExtensionAdapter[]>();
		private readonly List<PluginStartInfo> manualCaptureExtensionSettings = new List<PluginStartInfo>();
		protected static readonly Dictionary<string, List<string>> compositePluginIds = new Dictionary<string, List<string>>();

		private ICaptureExtensionAdapter[] currentCaptureExtensions;
		protected ICaptureExtensionAdapter[] CurrentCaptureExtensions
		{
			get { lock (thisLock) { return currentCaptureExtensions; } }
			private set { lock (thisLock) { currentCaptureExtensions = value; } }
		}

		protected abstract ICaptureExtensionAdapter GetCaptureExtensionWithId(PluginStartInfo pluginStartInfo);

		protected PluginCaptureService()
		{
			CurrentCaptureExtensions = emptyExtensions;
		}

		private bool IsAsyncCapturingRequired()
		{
			var newValue = ConfigManager.AsyncPluginsEnabled;
			if (newValue != useAsyncCapturing)
			{
				log.Debug("Async capturing changed to " + newValue);
				useAsyncCapturing = newValue;
				RefreshCaptureExtensionsAsync();
				if (!useAsyncCapturing)
				{
					parallelCollection.Clear();
				}
			}

			return useAsyncCapturing;
		}

		public void SetCaptureExtensions(List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow, Dictionary<string, string> globalVariables)
		{
			var localCaptureExtensions = CurrentCaptureExtensions;
			if (localCaptureExtensions == emptyExtensions) return;
			lock (thisLock) //we have to make sure localCaptureExtensions are not Disposed while in use
			{
				if (isDisposed) return;
				captureExtensionsInUse.Add(localCaptureExtensions);
			}
			try
			{
				if (IsAsyncCapturingRequired())
				{
					SetCaptureExtensionsAsyncImpl(localCaptureExtensions, windowsInfo, shouldCaptureWindow, globalVariables);
				}
				else
				{
					SetCaptureExtensionsImpl(localCaptureExtensions, windowsInfo, shouldCaptureWindow);
				}
				foreach (var window in windowsInfo.Where(w => w.CaptureExtensions != null).ToList())
				{
					foreach (var extension in window.CaptureExtensions.ToList())
					{
						foreach (var compositePluginId in compositePluginIds)
						{
							if (compositePluginId.Value.Contains(extension.Key.Id))
								window.CaptureExtensions.Add(new CaptureExtensionKey(compositePluginId.Key, extension.Key.Key), extension.Value);
						}
					}
				}
			}
			finally
			{
				lock (thisLock)
				{
					captureExtensionsInUse.Remove(localCaptureExtensions);
					if (numInUseNotificationReq != 0)
					{
						Monitor.PulseAll(thisLock); //signal that we've finished using localCaptureExtensions so it can be disposed (if there are waiters)
					}
				}
			}
		}

		private void SetCaptureExtensionsImpl(ICaptureExtensionAdapter[] localCaptureExtensions, List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow)
		{
			foreach (var captureExtension in localCaptureExtensions)
			{
				try
				{
					captureExtension.SetCaptureExtensions(windowsInfo, shouldCaptureWindow);
				}
				catch (Exception ex)
				{
					log.Error("Failed to set plugin data", ex);
				}
			}
		}

		private void SetCaptureExtensions(List<DesktopWindow> windowsInfo,
			IEnumerable<Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]>> capturedValues, bool requireCache)
		{
			foreach (var pluginResult in capturedValues.Where(x => x != null))
			{
				if (requireCache)
				{
					var pluginFirstVal = pluginResult.Values.SelectMany(x => x).FirstOrDefault();
					if (pluginFirstVal.Key != null)
					{
						var pluginId = pluginFirstVal.Key.Id;
						Debug.Assert(pluginResult.Values.SelectMany(x => x).All(x => x.Key.Id == pluginId));
						cachedPluginValues.Set(pluginId, pluginResult);
					}
				}

				foreach (var desktopWindow in windowsInfo)
				{
					KeyValuePair<CaptureExtensionKey, string>[] captures;
					if (pluginResult.TryGetValue(desktopWindow.Handle, out captures))
					{
						foreach (var capture in captures)
						{
							desktopWindow.SetCaptureExtension(capture.Key, capture.Value);
						}
					}
				}
			}
		}

		private void SetCaptureExtensionsAsyncImpl(ICaptureExtensionAdapter[] localCaptureExtensions, List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow, Dictionary<string, string> globalVariables)
		{
			try
			{
				parallelCollection.Set(localCaptureExtensions);
				//todo This only works on Windows atm
				var results = parallelCollection.MapOrDefault(x => ((ICaptureExtensionAdapter)x).Capture(windowsInfo, shouldCaptureWindow), TimeSpan.FromMilliseconds(ConfigManager.RuleMatchingInterval / 2));
				SetCaptureExtensions(windowsInfo, results, true);

				var failedResults = new List<Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]>>();
				foreach (var failedPlugin in parallelCollection.GetFailed())
				{
					Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]> capture;
					if (cachedPluginValues.TryGetValue(failedPlugin.CaptureExtensionSettings.PluginId, out capture))
					{
						failedResults.Add(capture);
					}
				}

				SetCaptureExtensions(windowsInfo, failedResults, false);

				var frozenPlugins = parallelCollection.GetFailed(ConfigManager.PluginFailThreshold);
				if (frozenPlugins.Length > 0)
				{
					var frozenPluginNameArr = frozenPlugins.Select(x => x.CaptureExtensionSettings.PluginId).ToArray();
					var frozenPluginsStr = string.Join(", ", frozenPluginNameArr);
					globalVariables.Add("FrozenPlugin", frozenPluginsStr);
					TelemetryHelper.Observe(TelemetryHelper.KeyFrozenPlugin, frozenPluginsStr);
				}
			}
			catch (Exception ex)
			{
				log.Error("Failed to set plugin data", ex);
			}
		}

		//todo LoadCaptureExtensionsFromCensorRulesAsync or proper loading with params from an other source
		public void LoadCaptureExtensionsFromWorkDetectorRulesAsync(WorkDetectorRule[] rules)
		{
			RefreshCaptureExtensionsAsync(rules, null);
		}

		public void LoadCaptureExtensionsFromCollectorRulesAsync(CollectorRule[] rules)
		{
			RefreshCaptureExtensionsAsync(null, rules);
		}

		public void UnregisterCaptureExtensionSettings(PluginStartInfo settings)
		{
			lock (thisLock)
			{
				if (isDisposed) return;
				manualCaptureExtensionSettings.Remove(settings);
			}
			RefreshCaptureExtensionsAsync();
		}

		public void RegisterCaptureExtensionSettings(PluginStartInfo settings)
		{
			lock (thisLock)
			{
				if (isDisposed) return;
				manualCaptureExtensionSettings.Add(settings);
			}
			RefreshCaptureExtensionsAsync();
		}

		private CollectorRule[] lastCollectorRules;
		private WorkDetectorRule[] lastRules;
		//this is a hax atm. but foolproof
		private void RefreshCaptureExtensionsAsync(WorkDetectorRule[] rules = null, CollectorRule[] collectorRules = null)
		{
			int version;
			WorkDetectorRule[] currentRules;
			CollectorRule[] currentCollectorRules;
			PluginStartInfo[] currentManualSettings;
			lock (thisLock)
			{
				if (isDisposed) return;
				if (rules != null)
				{
					lastRules = rules;
				}
				if (collectorRules != null)
				{
					lastCollectorRules = collectorRules;
				}
				currentRules = lastRules;
				currentCollectorRules = lastCollectorRules;
				currentManualSettings = manualCaptureExtensionSettings.ToArray();
				version = ++currentRuleVersion;
			}
			//load data on bg thread (this might take a while)
			ThreadPool.QueueUserWorkItem(n =>
			{
				ICaptureExtensionAdapter[] current;
				lock (thisLock)
				{
					if (isDisposed) return;
					if (currentRuleVersion != version) return; //drop old data
					current = CurrentCaptureExtensions;
					numBgRefreshRunning++; //indicate that sg is running on a bg thread (and we must wait before exiting the program)
				}
				try
				{
					List<PluginStartInfo> settings;
					var changed = GetCaptureExtensionSettingsIfChanged(currentRules, currentCollectorRules, currentManualSettings, current, out settings);
					if (!changed) return; //skip loading if nothing has changed
					log.Info("Plugins changed");
					var adapters = GetCaptureExtensionsFromSettingsNoThrow(settings);
					ICaptureExtensionAdapter[] adaptersToDispose;
					lock (thisLock)
					{
						if (currentRuleVersion != version)
						{
							adaptersToDispose = adapters; //drop old data (lost the race)
						}
						else
						{
							adaptersToDispose = CurrentCaptureExtensions;
							CurrentCaptureExtensions = adapters;
							WaitUntilAdaptersCanBeDisposed(adaptersToDispose);
						}
					}
					DisposeAdaptersNoThrow(adaptersToDispose);
				}
				finally
				{
					lock (thisLock)
					{
						if (--numBgRefreshRunning == 0 && isDisposed)
						{
							Monitor.PulseAll(thisLock); //indicate if dispose is waiting for the last bg task to finish
						}
					}
				}
			}
										 , null);
		}

		private void WaitUntilAdaptersCanBeDisposed(ICaptureExtensionAdapter[] adaptersToDispose)
		{
			if (adaptersToDispose == emptyExtensions) return; //we don't have to wait for that
			//Assert that lock is held
			//Assert (CurrentCaptureExtensions != adaptersToDispose) or CurrentCaptureExtensions won't be used anymore (isDisposed)
			var delayedDispose = false;
			try
			{
				while (captureExtensionsInUse.Contains(adaptersToDispose))
				{
					if (!delayedDispose)
					{
						delayedDispose = true;
						numInUseNotificationReq++; //indicate that we want a notification if captureExtensionsInUse is changed
					}
					Monitor.Wait(thisLock);
				}
			}
			finally
			{
				if (delayedDispose)
				{
					numInUseNotificationReq--;
				}
			}
		}

		private static void DisposeAdaptersNoThrow(ICaptureExtensionAdapter[] oldAdapters)
		{
			if (oldAdapters == null || oldAdapters.Length == 0) return;
			foreach (var adapter in oldAdapters)
			{
				Debug.Assert(adapter != null);
				try
				{
					adapter.Dispose();
				}
				catch (Exception ex)
				{
					log.Error("Unable to dispose Capture Extension Adapter", ex);
				}
			}
		}

		private bool GetCaptureExtensionSettingsIfChanged(WorkDetectorRule[] rules, CollectorRule[] collectorRules, PluginStartInfo[] currentManualSettings, ICaptureExtensionAdapter[] current, out List<PluginStartInfo> settings)
		{
			settings = GetCaptureExtensionSettingsFromPluginRules(rules)
				.Concat(GetCaptureExtensionSettingsFromPluginRules(collectorRules))
				.Concat(GetInternalCaptureExtensionSettings())
				.Concat(currentManualSettings)
				.Distinct()
				.ToList();
			return !current.Select(m => m.CaptureExtensionSettings).SequenceEqual(settings);
		}

		protected abstract IEnumerable<PluginStartInfo> GetInternalCaptureExtensionSettings();

		private static IEnumerable<PluginStartInfo> GetCaptureExtensionSettingsFromPluginRules(IEnumerable<IPluginRule> rules)
		{
			if (rules == null) yield break;
			foreach (var rule in rules)
			{
				if (rule.ExtensionRules != null)
				{
					foreach (var kvPair in rule.ExtensionRules)
					{
						List<ExtensionRuleParameter> extParams = null;
						rule.ExtensionRuleParametersById?.TryGetValue(kvPair.Key.Id, out extParams);
						foreach (var id in compositePluginIds.TryGetValue(kvPair.Key.Id, out var innerIds) ? innerIds.ToArray() : new [] { kvPair.Key.Id } )
						{
							var startInfo = new PluginStartInfo { PluginId = id, Details = new PluginStartInfoDetails { Rule = rule }, Parameters = extParams };
							yield return startInfo;
						}
					}
				}
				if (rule.ExtensionRuleParametersById != null)
				{
					foreach (var kvPair in rule.ExtensionRuleParametersById)
					{
						foreach (var id in compositePluginIds.TryGetValue(kvPair.Key, out var innerIds) ? innerIds.ToArray() : new[] { kvPair.Key })
						{
							var startInfo = new PluginStartInfo { PluginId = id, Parameters = kvPair.Value, Details = new PluginStartInfoDetails { Rule = rule } };
							yield return startInfo;
						}
					}
				}
				if (rule.Children == null) continue;
				foreach (var info in GetCaptureExtensionSettingsFromIRules(rule.Children.OfType<IRule>())) //warning nested iterators
				{
					yield return info;
				}
			}
		}

		private static IEnumerable<PluginStartInfo> GetCaptureExtensionSettingsFromIRules(IEnumerable<IRule> rules)
		{
			if (rules == null) yield break;
			foreach (var rule in rules)
			{
				if (rule.ExtensionRules == null) continue;
				foreach (var kvPair in rule.ExtensionRules)
				{
					foreach (var id in compositePluginIds.TryGetValue(kvPair.Key.Id, out var innerIds) ? innerIds.ToArray() : new[] { kvPair.Key.Id })
					{
						var startInfo = new PluginStartInfo() { PluginId = id, }; //no params here
						yield return startInfo;
					}
				}
				if (rule.Children != null)
				{
					foreach (var info in GetCaptureExtensionSettingsFromIRules(rule.Children)) //warning nested iterators
					{
						yield return info;
					}
				}
			}
		}

		internal ICaptureExtensionAdapter[] GetCaptureExtensionsFromSettingsNoThrow(List<PluginStartInfo> idsToLoad)
		{
			try
			{
				return idsToLoad == null || idsToLoad.Count == 0
						? emptyExtensions
						: GetCaptureExtensionWithIds(idsToLoad).ToArray();
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in GetCaptureExtensionsFromWorkDetectorRules", ex);
				return emptyExtensions;
			}
		}

		private IEnumerable<ICaptureExtensionAdapter> GetCaptureExtensionWithIds(IEnumerable<PluginStartInfo> captureExtIds)
		{
			foreach (var captureExtId in captureExtIds)
			{
				ICaptureExtensionAdapter captureExtension;
				try
				{
					captureExtension = GetCaptureExtensionWithId(captureExtId);
				}
				catch (Exception ex)
				{
					log.Error("Unable to load extension with id " + captureExtId, ex);
					captureExtension = null;
				}
				if (captureExtension == null) continue;
				yield return captureExtension;
			}
		}

		private bool isDisposed;
		public void Dispose()
		{
			ICaptureExtensionAdapter[] adaptersToDispose;
			lock (thisLock)
			{
				++currentRuleVersion; //immediately drop cap ext generatend on bg thread
				if (isDisposed) return;
				isDisposed = true;
				while (numBgRefreshRunning > 0) //have to wait until BG refreshes have finished
				{
					log.Info("Waiting for " + numBgRefreshRunning + " bg tasks to finish");
					Monitor.Wait(thisLock);
				}
				adaptersToDispose = CurrentCaptureExtensions;
				CurrentCaptureExtensions = emptyExtensions;
				WaitUntilAdaptersCanBeDisposed(adaptersToDispose); //make sure that we can dispose CurrentCaptureExtensions (SetCaptureExtensions is not running)
			}
			DisposeAdaptersNoThrow(adaptersToDispose);
		}
	}
}
