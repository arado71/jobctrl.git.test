using System.Text.RegularExpressions;

namespace Ocr.Recognition
{
	public class RecognitionProcessor
	{
		private readonly Regex regex;
		private readonly bool isCaseInsensitive;

		public RecognitionProcessor(string regexPattern, bool ignoreCase)
		{
			if (!string.IsNullOrWhiteSpace(regexPattern))
			{
				regex = new Regex(regexPattern);
			}
			isCaseInsensitive = ignoreCase;
		}

		public string Process(string input)
		{
			var ret = input;
			if (regex != null)
			{
				ret = regex.Match(ret).ToString();
			}
			if (isCaseInsensitive)
			{
				ret =  ret.ToLowerInvariant();
			}
			return ret;
		}
	}
}
