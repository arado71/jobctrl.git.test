using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Search
{
	public abstract class StringMatcher
	{
		protected static char[] whiteSpaces = new[] { ' ', '\t', '\r', '\n', }; //todo 85 unicode etc...

		private readonly Dictionary<int, string> textDict = new Dictionary<int, string>();
		protected Dictionary<int, string> TextDict { get { return textDict; } }


		public virtual void Add(int id, string text)
		{
			if (string.IsNullOrEmpty(text)) throw new ArgumentOutOfRangeException();
			textDict.Add(id, text); //will throw on dupes
		}

		public virtual void Clear()
		{
			textDict.Clear();
		}

		public abstract IEnumerable<int> GetMatches(string text);

		protected static string RemoveDiacritics(string src)
		{
			string stFormD = src.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder();

			for (int ich = 0; ich < stFormD.Length; ich++)
			{
				UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
				if (uc != UnicodeCategory.NonSpacingMark)
				{
					sb.Append(stFormD[ich]);
				}
			}

			return (sb.ToString().Normalize(NormalizationForm.FormC));
		}
	}
}
