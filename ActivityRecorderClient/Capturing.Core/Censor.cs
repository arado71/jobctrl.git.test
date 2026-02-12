using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Rules;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Thread-safe class for checking whether we need to censor some data.
	/// </summary>
	/// <remarks>
	/// In theory a race can occur when calling RulesChanged several times and slower old data might overwrite faster new one.
	/// But that's ok we won't call that more than once at a time.
	/// </remarks>
	public class Censor
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly RuleMatcher<CensorRule>[] emptyRules = new RuleMatcher<CensorRule>[0];

		private readonly object thisLock = new object();

		private RuleMatcher<CensorRule>[] currentRules = emptyRules;
		public RuleMatcher<CensorRule>[] CurrentRules
		{
			get { lock (thisLock) { return currentRules; } }
			set { lock (thisLock) { currentRules = value; } }
		}

		public void SetCensorRules(List<CensorRule> rules)
		{
			RulesChangedImpl(rules);
		}

		private void RulesChangedImpl(List<CensorRule> rules)
		{
			if (rules == null || rules.Count == 0)
			{
				CurrentRules = emptyRules;
				log.Info("Censor rules are Empty");
				return;
			}
			var newRules = rules
				.Select(n =>
				{
					try
					{
						return new RuleMatcher<CensorRule>(n);
					}
					catch (Exception ex)
					{
						log.Error("Invalid censor rule: " + n.ToString(), ex);
						return null;
					}
				})
				.Where(n => n != null)
				.ToArray();
			CurrentRules = newRules;
			if (newRules.Length == 0)
			{
				log.Info("Censor rules are Empty");
			}
			else
			{
				foreach (var newRule in newRules)
				{
					log.Info("Loaded censor rule " + newRule.Rule);
				}
			}
		}

		public bool CensorCaptureIfApplicable(DesktopCapture desktopCapture)
		{
			if (desktopCapture == null || desktopCapture.DesktopWindows == null) return false;
			var censored = false;
			foreach (var desktopWindow in desktopCapture.DesktopWindows) //we censor all windows
			{
				censored |= CensorWindowIfApplicable(desktopWindow, desktopCapture);
			}
			return censored;
		}

		public CensorRuleType GetCensorRuleTypeForWindow(DesktopWindow desktopWindow, DesktopCapture desktopCapture)
		{
			var currRules = CurrentRules;
			var censorType = currRules
				.Where(n => n.IsMatch(desktopWindow, desktopCapture))
				.Aggregate(CensorRuleType.None, (prev, n) => prev | n.Rule.RuleType);
			return censorType;
		}

		private bool CensorWindowIfApplicable(DesktopWindow desktopWindow, DesktopCapture desktopCapture)
		{
			var ruleType = GetCensorRuleTypeForWindow(desktopWindow, desktopCapture);
			var censored = false;
			if ((ruleType & CensorRuleType.HideTitle) != 0)
			{
				desktopWindow.Title = "*C*";
				censored = true;
			}
			if ((ruleType & CensorRuleType.HideUrl) != 0)
			{
				desktopWindow.Url = "*C*";
				censored = true;
			}
			if ((ruleType & CensorRuleType.HideScreenShot) != 0)
			{
				if (desktopCapture.Screens != null)
				{
					foreach (var screen in desktopCapture.Screens)
					{
						if (screen.OriginalScreenImage == null) continue;
						screen.Extension = "*C*";
						screen.OriginalScreenImage = null;
						censored = true;
					}
				}
			}
			else if ((ruleType & CensorRuleType.HideWindow) != 0) //we either remove screens or hide window
			{
				if (desktopCapture.Screens != null)
				{
					foreach (var screen in desktopCapture.Screens)
					{
						if (screen.OriginalScreenImage == null) continue;
						HideWindow(desktopWindow, desktopCapture);
						censored = true;
					}
				}
			}
			return censored;
		}

		private void HideWindow(DesktopWindow desktopWindow, DesktopCapture desktopCapture)
		{
			if (desktopCapture.Screens == null) return;
			if (desktopCapture.Screens.All(n => n.OriginalScreenImage == null)) return;
			foreach (var screen in desktopCapture.Screens)
			{
				using (var area = GetWindowAreaForScreen(screen.Bounds, desktopWindow, desktopCapture))
				{
					if (area == null) continue;
					using (var graphics = Graphics.FromImage(screen.OriginalScreenImage))
					{
						graphics.FillRegion(Brushes.Black, area); //not ideal to pollute threadpool threads
					}
				}
			}

		}

		private static Region GetWindowAreaForScreen(Rectangle rectangle, DesktopWindow desktopWindow, DesktopCapture desktopCapture)
		{
			var result = new Region(TransformOrigin(desktopWindow.WindowRect, rectangle.Location));
			foreach (var window in desktopCapture.DesktopWindows.TakeWhile(n => n != desktopWindow))
			{
				result.Exclude(TransformOrigin(window.WindowRect, rectangle.Location));
			}
			result.Intersect(TransformOrigin(rectangle, rectangle.Location));
			return result;
		}

		private static Rectangle TransformOrigin(Rectangle original, Point negVector)
		{
			var result = original;
			result.Offset(-negVector.X, -negVector.Y);
			return result;
		}
	}
}
