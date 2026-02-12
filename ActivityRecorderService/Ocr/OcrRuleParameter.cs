using MathNet.Numerics.LinearAlgebra.Double;
using Ocr.Learning;
using Ocr.Model;

namespace Tct.ActivityRecorderService.Ocr
{
	public class OcrRuleParameter
	{
		public int RuleId { get; set; }
		public int CompanyId { get; set; }
		private string processName;
		public string ProcessName { get { return processName.Replace("\\", ""); } set { processName = value; } }
		public string Language { get; set; }
		public LearningHelper.OcrCharSets CharSet { get; set; }
		public bool UserContribution { get; set; }
		public string RuleKey { get; set; }
		public OcrConfiguration OcrConfigurationDataAccordingToRuleKey { get; set; }
		public OcrConfig OcrConfig2DataAccordingToRuleKey { get; set; }
		public void Update(DenseVector dv)
		{
			OcrConfigurationDataAccordingToRuleKey.EngineParameters = dv.Values;
		}
	}
}
