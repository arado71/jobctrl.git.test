using log4net;
using MathNet.Numerics.LinearAlgebra.Double;
using Ocr.Helper;
using Ocr.Model;
using Ocr.Recognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Website;

namespace Tct.ActivityRecorderService.Ocr
{
	public class OcrRules
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(OcrRules));
		private readonly List<SnippetExt> items;
		public OcrRuleParameters Parameters { get; set; }
		public IEnumerable<int> Companies
		{
			get
			{
				return (from e in items
						group e by new { e.CompanyId }
							into g
						select g.Key.CompanyId).ToList();
			}
		}
		public IEnumerable<string> Processes
		{
			get
			{
				return (from e in Parameters
						group e by new { e.ProcessName }
							into g
						select g.Key.ProcessName).ToList();
			}
		}

		public IEnumerable<int> RuleIds
		{
			get
			{
				return (from e in Parameters
						group e by new { e.RuleId }
							into g
						select g.Key.RuleId).ToList();
			}
		}

		public OcrRules(List<SnippetExt> contributionItems)
		{
			items = contributionItems;
			Parameters = new OcrRuleParameters(contributionItems);
		}
		public List<SnippetExt> GetItems(Func<SnippetExt, bool> exp)
		{
			return items.Where(e => exp(e)).ToList();
		}
		public OcrRuleParameter GetFirst(Expression<Func<OcrRuleParameter, bool>> expr)
		{
			return Parameters.FirstOrDefault(expr.Compile());
		}
		internal OcrConfig UpdateConfig(int ruleId, int company, string process, DenseVector dv)
		{
			OcrConfig cfg = new TransformConfiguration(dv).ConvertDensevectorToOcrConfig();
			foreach (OcrRuleParameter p in Parameters.Where(e => e.RuleId == ruleId && e.ProcessName == process && e.CompanyId == company))
				p.Update(dv);
			foreach (OcrRuleParameter p in Parameters.Where(e => e.RuleId == ruleId))
			{
				cfg.SetAreaOfInterest(p.OcrConfigurationDataAccordingToRuleKey.Area);
				cfg.ProcessNameRegex = p.OcrConfigurationDataAccordingToRuleKey.ProcessName;
				cfg.Language = p.OcrConfigurationDataAccordingToRuleKey.Language;
				cfg.CharSet = p.OcrConfigurationDataAccordingToRuleKey.CharSet;
				cfg.UserContribution = p.OcrConfigurationDataAccordingToRuleKey.UserContribution;
				cfg.ContentRegex = p.OcrConfig2DataAccordingToRuleKey.ContentRegex;
				cfg.IgnoreCase = p.OcrConfig2DataAccordingToRuleKey.IgnoreCase;
				cfg.TitleRegex = p.OcrConfig2DataAccordingToRuleKey.TitleRegex;
				cfg.Interpolation = p.OcrConfig2DataAccordingToRuleKey.Interpolation;
				cfg.HAlign = p.OcrConfig2DataAccordingToRuleKey.HAlign;
				cfg.VAlign = p.OcrConfig2DataAccordingToRuleKey.VAlign;
				cfg.Status = StatusEnum.InProgress;
				cfg.SizeW = p.OcrConfig2DataAccordingToRuleKey.SizeW;
				cfg.SizeH = p.OcrConfig2DataAccordingToRuleKey.SizeH;
			}

			return cfg;
		}

		internal void UpdateConfigInDB(int ruleId, OcrConfig cfg)
		{
			log.Debug("OCR Updating DB rules");
#if DEBUG
			log.Debug("Skipped in debug");
			return;
#endif
			StringBuilder sb = new StringBuilder(512);
			var param = Parameters.Where(e => e.RuleId == ruleId).FirstOrDefault();
			sb.Append(param.RuleKey).Append("=");
			sb.Append(JsonHelper.SerializeData(cfg));
			sb.Append(";");

			using (var ws = new WebsiteClientWrapper())
			{
				var ret = ws.Client.UpdateRuleParameters(OcrEmailHelper.GetRegistratorUsersTicket(param.CompanyId), ruleId, "JobCTRL.Ocr", "Capture", sb.ToString());
				log.DebugFormat("RuleId: {0}, content: {1}", ruleId, sb.ToString());
				log.Debug("OCR DB rules updated with result " + ret);
			}
		}

	}
}
