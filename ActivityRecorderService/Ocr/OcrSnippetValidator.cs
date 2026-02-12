using log4net;
using Ocr.Learning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tct.ActivityRecorderService.Ocr
{
	public class OcrSnippetValidator
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(OcrSnippetValidator));
		private List<Guid> invalidIds;

		public OcrSnippetValidator(LearningHelper.OcrCharSets charSet, List<SnippetExt> snippets)
		{
			invalidIds = new List<Guid>();
			ProcessItems(charSet, snippets);
		}

		public bool HasInvalidContent()
		{
			log.DebugFormat("Found {0} invalid snippet(s)", invalidIds.Count);
			return invalidIds.Count > 0;
		}

		private void ProcessItems(LearningHelper.OcrCharSets charSet, List<SnippetExt> snippets)
		{
			foreach (var snippet in snippets)
			{
				if (LearningHelper.IsInputInvalid(charSet, snippet.Content))
				{
					invalidIds.Add(snippet.Guid);
				}
			}
		}

		public void MarkBadData()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var processDate = DateTime.Now;
				foreach (var itemId in invalidIds)
				{
					var rec = context.Snippets.SingleOrDefault(e => e.Guid == itemId);
					if (rec == null) continue;
					rec.IsBadData = true;
					rec.ProcessedAt = processDate;
					log.DebugFormat("Snippet {0} marked as BadData", itemId);
				}
				context.SubmitChanges();
			}
		}
		
	}
}
