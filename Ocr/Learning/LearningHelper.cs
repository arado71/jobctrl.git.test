using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Ocr.Learning
{
	public static class LearningHelper
	{

		public enum OcrFontFamily
		{
			Arial = 0,
			Consolas = 1,
			LucidaConsole = 2,
			SegoeUI = 3,
			Calibri = 4,
			Tahoma = 5,
			TimesNewRoman = 6,
			CourierNew = 7

		}

		public enum OcrCharSets
		{
			Numbers,
			NumbersAndCapitalLetters,
			Ascii7Bit,
			Hungarian,
			Portuguese
		}

		private const string SkipChars = " \n\r\t";
		private static readonly ILog log = LogManager.GetLogger(typeof(LearningHelper));

		private static readonly string[] CharGroups =
		{
			"abcdefghijklmnopqrstuvwxyz", // lowercase letter
			"ABCDEFGHIJKLMNOPQRSTUVWXYZ", // uppercase letter
			"áéíóöőúüű", // lowercase accented letter
			"ÁÉÍÓÖŐÚÜŰ", // uppercase accented letter
			"0123456789", // numbers
			".,-?!():", // common punctuation marks
			"/'\"[];", // literacy punctuation marks
			"+=%<>*", // numeric signs
			"{}\\_#&", // coding symbols
			"àèìòùãõâôç",
			"ÀÈÌÒÙÃÕÂÔÇ",
			"@$`´^~|"
		};

		public static string GetLearningText(OcrCharSets charSet)
		{
			string learningText = "";
			switch (charSet)
			{
				case OcrCharSets.Numbers:
					learningText = GetLearningText(string.Join("", "1"));
					break;
				case OcrCharSets.NumbersAndCapitalLetters:
					learningText = GetLearningText(string.Join("", "1A"));
					break;
				case OcrCharSets.Ascii7Bit:
					learningText = GetLearningText(string.Join("", "aA1./+&"));
					break;
				case OcrCharSets.Hungarian:
					learningText = GetLearningText(string.Join("", "aAáÁ1./+&"));
					break;
				case OcrCharSets.Portuguese:
					learningText = GetLearningText(string.Join("", "aAáÁ1./+&àÀ@"));
					break;
			}
			return learningText;

		}

		public static string GetLearningText(params string[] inputs)
		{
			return GetLearningText(string.Join("", inputs));
		}

		public static string GetLearningText(string input)
		{
			var imported = "";
			foreach (var c in input)
			{
				if (SkipChars.Contains(c)) continue;
				if (imported.Contains(c)) continue;
				var found = false;
				foreach (var charGroup in CharGroups)
					if (charGroup.Contains(c))
					{
						imported += charGroup;
						found = true;
					}

				if (!found) imported += c;
			}

			return imported;
		}

		public static Font GetFont(OcrFontFamily fontFamily, float size)
		{
			switch (fontFamily)
			{
				case OcrFontFamily.Arial:
					return new Font("Arial", size, FontStyle.Regular);
				case OcrFontFamily.Calibri:
					return new Font("Calibri", size, FontStyle.Regular);
				case OcrFontFamily.Consolas:
					return new Font("Consolas", size, FontStyle.Regular);
				case OcrFontFamily.LucidaConsole:
					return new Font("Lucida Console", size, FontStyle.Regular);
				case OcrFontFamily.SegoeUI:
					return new Font("Segoe UI", size, FontStyle.Regular);
				case OcrFontFamily.Tahoma:
					return new Font("Tahoma", size, FontStyle.Regular);
				case OcrFontFamily.TimesNewRoman:
					return new Font("Times new roman", size, FontStyle.Regular);
				case OcrFontFamily.CourierNew:
					return new Font("Courier New", size, FontStyle.Regular);
				default:
					return new Font("Times new roman", size, FontStyle.Regular);
			}
		}

		public static string[] GetCharSets()
		{
			return Enum.GetNames(typeof(OcrCharSets));
		}

		public static bool IsInputInvalid(OcrCharSets charSet, string input)
		{
			string extLearningText = GetLearningText(charSet) + " ";
			return !input.All((ch) => extLearningText.Contains(ch));
		}

	}
}