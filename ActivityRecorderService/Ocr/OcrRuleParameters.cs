using log4net;
using Ocr.Helper;
using Ocr.Model;
using Ocr.Recognition;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tct.ActivityRecorderService.Ocr
{
	public class OcrRuleParameters : List<OcrRuleParameter>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(OcrRuleParameters));
		private readonly IEnumerable<SnippetExt> _snippets;
		public OcrRuleParameters(IEnumerable<SnippetExt> snippets)
		{
			this._snippets = snippets;
			// rules touched by contribution items
			var ruleIds = from e in _snippets
							group e by new { e.RuleId, e.CompanyId, e.ProcessName }
								into rg
							select new { rg.Key.RuleId, rg.Key.CompanyId, rg.Key.ProcessName };

			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				foreach (var rule in ruleIds)
				{
					string ruleParameterString = context.GetPluginParameter(rule.RuleId);   // key=OcrConfig;...
					if (ruleParameterString == null)
					{
						log.ErrorFormat("Rule paramter cannot be empty. RuleId is " + rule);
						throw new ArgumentException();
					}

					if (!ruleParameterString.EndsWith(";")) ruleParameterString += ";";
					var regex = new Regex(@"((?<key>\w+)=(?<value>([^;]*)));");
					foreach (Match match in regex.Matches(ruleParameterString))
					{
						if (match.Groups["key"].Success && match.Groups["value"].Success)
						{
							OcrConfig config;       // rule parameter contains OcrConfig 
							JsonHelper.DeserializeData(match.Groups["value"].Value, out config);
							OcrConfiguration configuration = new OcrConfiguration();
							var tc = new TransformConfiguration(
								Brightness: config.Brightness,
								Contrast: config.Contrast,
								Scale: config.Scale,
								TresholdLimit: config.TresholdLimit,
								TresholdChannel: config.TresholdChannel,
								Interpolation: config.Interpolation);
							configuration.Area = new Rectangle(config.AreaX, config.AreaY, config.AreaW, config.AreaH);
							configuration.Language = config.Language;
							configuration.CharSet = config.CharSet;
							configuration.EngineParameters = tc.DenseVector.Values;
							configuration.ProcessName = config.ProcessNameRegex;
							configuration.UserContribution = config.UserContribution;
							var ruleParameter = new OcrRuleParameter
							{
								RuleId = rule.RuleId,
								RuleKey = match.Groups["key"].Value,
								CompanyId = rule.CompanyId,
								Language = config.Language ?? "eng",
								CharSet = config.CharSet,
								UserContribution = config.UserContribution,
								ProcessName = configuration.ProcessName ?? rule.ProcessName ?? ".*",
								OcrConfigurationDataAccordingToRuleKey = configuration,
								OcrConfig2DataAccordingToRuleKey = config
							};
							Add(ruleParameter);
						}
						else
							log.Error("Invalid configuration. Either `key` or `value` doesn't match", new ArgumentException(ruleParameterString));
					}
				}
			}
		}
		
	}
}
