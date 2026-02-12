using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	/// <summary>
	/// Class for coordinating collector rule detection, timestamping, aggregation and sending.
	/// </summary>
	//todo check null captures !
	public class CollectorCoordinator : IDisposable
	{
		//private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan StoppingSendTimeout = TimeSpan.FromSeconds(3);

		private readonly CollectedItemCreator itemCreator = new CollectedItemCreator();
		private readonly CollectedItemManager itemManager = new CollectedItemManager();
		private readonly CollectorRulesManager rulesManager = new CollectorRulesManager();
		private readonly CollectorRuleDetector ruleDetector = new CollectorRuleDetector();
		private readonly CaptureManager captureManager;
		private readonly AsyncWorkQueue<CollectedItem> itemSender; //marshal calls to bg thread (not water-tight but won't affect the UI)

		public bool IsEnabled { get { return ruleDetector.IsEnabled; } }
		public event EventHandler RulesChanged;

		public CollectorCoordinator(CaptureManager captureManager, WorkItemManager workItemManager, IPluginCaptureService pluginCaptureService)
		{
			itemCreator.ItemCreated += (sender, item) => itemSender.EnqueueAsync(item.Value);
			itemSender = new AsyncWorkQueue<CollectedItem>(n => itemManager.Add(n));
			itemManager.ItemCreated += (sender, args) =>
			{
				if (args.ShouldSendImmediately)
				{
					workItemManager.SendOrPersist(args.Item, StoppingSendTimeout);
				}
				else
				{
					workItemManager.PersistAndSend(args.Item);
				}
			};
			this.captureManager = captureManager;
			rulesManager.RulesChanged += (sender, item) =>
			{
				pluginCaptureService.LoadCaptureExtensionsFromCollectorRulesAsync((item.Value.Rules ?? new List<CollectorRule>(0)).ToArray());
				ruleDetector.SetCollectorRules(item.Value);
				RulesChanged?.Invoke(this, EventArgs.Empty);
			};
		}

		public CollectorRuleDetectorResult DetectCaptures(DesktopCapture desktopCapture)
		{
			DebugEx.EnsureBgThread();
			return ruleDetector.DetectCaptures(desktopCapture);
		}

		public IDisposable GetTimeStamper(CollectorRuleDetectorResult collectorRuleDetectorResult)
		{
			DebugEx.EnsureGuiThread();
			return new TimeStamper(this, collectorRuleDetectorResult.CapturedValues);
		}

		public void UpdateImmediate(DateTime dateTime, Dictionary<string, string> capturedValues)
		{
			itemCreator.UpdateCapturedValues(dateTime, capturedValues);
		}

		public void Start()
		{
			itemManager.Load();
			rulesManager.LoadRules();
			rulesManager.Start();
			itemManager.Start();
		}

		public void Stop()
		{
			rulesManager.Stop();
			itemManager.Stop();
		}

		public void Dispose()
		{
			itemSender.Dispose();
		}

		private sealed class TimeStamper : IDisposable //used on the gui thread without interruption
		{
			private readonly CollectorCoordinator parent;
			private readonly CaptureManager.WorkItemRelativeTime start;
			private readonly Dictionary<string, string> capturedValues;

			public TimeStamper(CollectorCoordinator parent, Dictionary<string, string> capturedValues)
			{
				DebugEx.EnsureGuiThread();
				this.parent = parent;
				this.capturedValues = capturedValues;
				start = parent.captureManager.GetWorkItemRelativeTime();
			}

			public void Dispose()
			{
				DebugEx.EnsureGuiThread();
				var end = parent.captureManager.GetWorkItemRelativeTime();

				if (end != null) //working after rules are executed
				{
					var dateTime = start == null || end.WorkId != start.WorkId ? end.FirstStartDate : end.GetNow();
					capturedValues.TryGetValue("ProcessName", out var pname);
					if (pname != "Idle") // ignore collecting for Idle process
						parent.itemCreator.UpdateCapturedValues(dateTime, capturedValues);
				}
				else //not working after rules are executed
				{
					//we could null out every captured key but that would increase the amount of data we should handle at the server
					//also we don't detect when user stops working manually and we should also null out everything to be consistent
					//we cannot use real captures when StopWorkRule was executed because then we would log things which should not be logged
				}
#if !DEBUG
			}
#else
				GC.SuppressFinalize(this);
			}

			~TimeStamper()
			{
				Debug.Fail("TimeStamper leaked");
			}
#endif
		}
	}
}
