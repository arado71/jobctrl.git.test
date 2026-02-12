using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public class WorkDetectorRulesDataSource : NSTableViewDataSource
	{
		private List<WorkDetectorRule> rules = new List<WorkDetectorRule>();

		public List<WorkDetectorRule> Rules { get { return rules; } }

		public WorkDetectorRulesDataSource(IEnumerable<WorkDetectorRule> rules)
		{
			if (rules == null)
				return;
			this.rules = rules.ToList();
		}

		public override int GetRowCount(MonoMac.AppKit.NSTableView tableView)
		{
			return rules.Count;
		}

		private static NSNumber OnState = NSNumber.FromInt32((int)NSCellStateValue.On);
		private static NSNumber OffState = NSNumber.FromInt32((int)NSCellStateValue.Off);

		public override NSObject GetObjectValue(NSTableView tableView, NSTableColumn tableColumn, int rowIndex)
		{
			var valueKey = (string)(NSString)tableColumn.Identifier;
			switch (valueKey)
			{
				case "Name":
					return (NSString)rules[rowIndex].Name;
				case "RuleType":
					return (NSString)RuleManagementService.GetShortNameFor(rules[rowIndex].RuleType);
				case "TitleRule":
					return (NSString)rules[rowIndex].TitleRule;
				case "ProcessRule":
					return (NSString)rules[rowIndex].ProcessRule;
				case "UrlRule":
					return (NSString)rules[rowIndex].UrlRule;
				case "RelatedId":
					return (NSString)rules[rowIndex].RelatedId.ToString();
				case "IsRegex":
					return rules[rowIndex].IsRegex ? OnState : OffState;
				case "IgnoreCase":
					return rules[rowIndex].IgnoreCase ? OnState : OffState;
				case "IsEnabled":
					return rules[rowIndex].IsEnabled ? OnState : OffState;
				case "IsPermanent":
					return rules[rowIndex].IsPermanent ? OnState : OffState;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override void SetObjectValue(NSTableView tableView, NSObject theObject, NSTableColumn tableColumn, int rowIndex)
		{
			var valueKey = (string)(NSString)tableColumn.Identifier;
			switch (valueKey)
			{
				case "Name":
					rules[rowIndex].Name = (NSString)theObject;
					break;
				case "RuleType":
					/*rules[rowIndex].RuleType = (WorkDetectorRuleType)Enum.Parse(
						typeof(WorkDetectorRuleType),
						(NSString)theObject
					);*/
					break;
				case "TitleRule":
					rules[rowIndex].TitleRule = (NSString)theObject;
					break;
				case "ProcessRule":
					rules[rowIndex].ProcessRule = (NSString)theObject;
					break;
				case "UrlRule":
					rules[rowIndex].UrlRule = (NSString)theObject;
					break;
				case "RelatedId":
					rules[rowIndex].RelatedId = int.Parse((NSString)theObject);
					break;
				case "IsRegex":
					rules[rowIndex].IsRegex = OnState.IsEqualToNumber((NSNumber)theObject);
					break;
				case "IgnoreCase":
					rules[rowIndex].IgnoreCase = OnState.IsEqualToNumber((NSNumber)theObject);
					break;
				case "IsEnabled":
					rules[rowIndex].IsEnabled = OnState.IsEqualToNumber((NSNumber)theObject);
					break;
				case "IsPermanent":
					rules[rowIndex].IsPermanent = OnState.IsEqualToNumber((NSNumber)theObject);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}

