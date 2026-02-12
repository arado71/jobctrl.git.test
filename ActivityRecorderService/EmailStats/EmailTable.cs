using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace Tct.ActivityRecorderService.EmailStats
{
	public class EmailTable
	{
		public int ColumnCount { get; private set; }
		public int RowCount { get { return rows.Count; } }
		public string Title { get; set; }

		private IList<CellData> header;
		private readonly List<IList<CellData>> rows = new List<IList<CellData>>();
		private readonly Dictionary<int, Style> columnStyles = new Dictionary<int, Style>();

		private const string HtmlTableStyle = " BORDER=\"1\" cellspacing=\"0\" cellpadding=\"2\" style=\"background-color: rgb(244, 244, 235); text-align:left; border-collapse:collapse; border:1px solid rgb(159, 154, 129);\" rules=\"cols\"";
		private const string HtmlAlterRowStyle = " style=\"background-color: rgb(234, 231, 217);\"";
		private const string HtmlHeaderRowStyle = " style=\"background-color: rgb(216, 212, 189); text-align:center; white-space:nowrap;\"";
		private const string HtmlNoWrapStyle = " style=\"white-space:nowrap;\"";

		public void SetHeader(params string[] headerValues)
		{
			header = headerValues.Select(n => CellData.CreateFrom(n)).ToArray();
			AdjustColumnCount(headerValues.Length);
		}

		public void SetColumnStyle(int columnNumber, Style columnStyle)
		{
			if (columnStyles.ContainsKey(columnNumber))
			{
				columnStyles[columnNumber] = columnStyle;
			}
			else
			{
				columnStyles.Add(columnNumber, columnStyle);
			}
		}

		public void AddRow()
		{
			rows.Add(new CellData[0]);
		}

		public void AddRow(params string[] rowValues)
		{
			rows.Add(rowValues.Select(n => CellData.CreateFrom(n)).ToArray());
			AdjustColumnCount(rowValues.Length);
		}

		public void AddRow(params CellData[] rowValues)
		{
			rows.Add(rowValues); //make a copy ?
			AdjustColumnCount(rowValues.Length);
		}

		private void AdjustColumnCount(int colCount)
		{
			if (ColumnCount < colCount) ColumnCount = colCount;
		}

		private int GetMaxAsciiColumnLength(int columnNum, bool isHeaderIncluded)
		{
			return GetMaxColumnLengthImpl(GetAsciiLength, columnNum, isHeaderIncluded);
		}

		private bool IsHtmlColumnEmpty(int columnNum)
		{
			return GetMaxColumnLengthImpl(GetHtmlLength, columnNum, false) == 0;
		}

		private static int GetAsciiLength(CellData cellData)
		{
			return cellData != null && cellData.AsciiValue != null ? cellData.AsciiValue.Length : 0;
		}

		private static int GetHtmlLength(CellData cellData)
		{
			return cellData != null && cellData.HtmlValue != null ? cellData.HtmlValue.Length : 0;
		}

		private int GetMaxColumnLengthImpl(Func<CellData, int> lengthFunc, int columnNum, bool isHeaderIncluded)
		{
			var maxRowLen = rows
				.Select(n => columnNum < n.Count ? lengthFunc(n[columnNum]) : 0)
				.DefaultIfEmpty(0)
				.Max();

			if (!isHeaderIncluded) return maxRowLen;

			var maxHeaderLen =
				(header != null
				&& columnNum < header.Count)
					? lengthFunc(header[columnNum]) + 2 //we need at least one whitespace on both ends
					: 0;

			return Math.Max(maxRowLen, maxHeaderLen);
		}

		public void GetHtmlTable(StringBuilder sb)
		{
			var htmlStyles = new InnerHtmlColumnStyle[ColumnCount];
			for (int i = 0; i < ColumnCount; i++)
			{
				htmlStyles[i] = new InnerHtmlColumnStyle();
				htmlStyles[i].IsHtmlColumnEmpty = IsHtmlColumnEmpty(i);
				Style colStyle;
				if (columnStyles.TryGetValue(i, out colStyle))
				{
					htmlStyles[i].ColumnStyle = colStyle;
				}
			}
			if (Title != null)
			{
				sb.Append("<TABLE border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><TR><TD style=\"text-align:center;\"><B><EM>").Append(HttpUtility.HtmlEncode(Title)).Append("</EM></B></TD></TR><TR><TD>");
			}
			sb.Append("<TABLE" + HtmlTableStyle + ">");
			//<CAPTION> is not working in gmail (its converted to a td, so first column is big for long captions)
			//if (Title != null)
			//{
			//    sb.Append("<CAPTION><B><EM>").Append(HttpUtility.HtmlEncode(Title)).Append("</EM></B></CAPTION>");
			//}
			if (header != null)
			{
				sb.Append("<TR" + HtmlHeaderRowStyle + ">");
				AppendEveryHtmlElement(sb, "TH", header, htmlStyles, true);
				sb.AppendLine("</TR>");
			}
			for (int i = 0; i < rows.Count; i++)
			{
				if (i % 2 == 1)
				{
					sb.Append("<TR" + HtmlAlterRowStyle + ">");
				}
				else
				{
					sb.Append("<TR>");
				}
				AppendEveryHtmlElement(sb, "TD", rows[i], htmlStyles, false);
				sb.AppendLine("</TR>");
			}
			sb.Append("</TABLE>");
			if (Title != null)
			{
				sb.Append("</TD></TR></TABLE>");
			}
		}

		private void AppendEveryHtmlElement(StringBuilder sb, string elementName, IList<CellData> values, InnerHtmlColumnStyle[] htmlStyles, bool isHeader)
		{
			for (int i = 0; i < ColumnCount; i++)
			{
				if (htmlStyles[i].IsHtmlColumnEmpty) continue; //don't display empty columns
				var colStyle = htmlStyles[i].ColumnStyle;
				var cellStyle = i < values.Count && values[i] != null ? values[i].CellStyle : null;
				var currentStyle = GetHtmlCellStyleWithDefaults(colStyle, cellStyle);
				var value = i < values.Count && values[i] != null && !string.IsNullOrEmpty(values[i].HtmlValue)
					? values[i].HtmlValue
					: "";
				var tdStyle = "";
				if (!isHeader //we don't format headers atm.
					&& value != "") //don't care about style on empty cell (reduce size of html)
				{
					tdStyle = GetHtmlStyleString(currentStyle);
					if (currentStyle.EscapeSpace.Value)
					{
						value = value.Replace(" ", "&nbsp;");
					}
				}
				sb.Append("<").Append(elementName).Append(tdStyle).Append(">")
					.Append(value)
					.Append("</").Append(elementName).Append(">");
			}
		}

		private static string GetHtmlStyleString(HtmlStyle currentStyle)
		{
			Debug.Assert(currentStyle != null); //and every Propery has a value
			var result = "";
			switch (currentStyle.Align.Value)
			{
				case TextAlign.Left: //default align in the table
					break;
				case TextAlign.Right:
					result += "text-align: right;";
					break;
				case TextAlign.Center:
					result += "text-align: center;";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			result = result
				+ (currentStyle.Bold.Value ? "font-weight:bold;" : "")
				+ (currentStyle.Italic.Value ? "font-style:italic;" : "")
				+ (currentStyle.NoWrap.Value ? "white-space:nowrap;" : "")
				+ (currentStyle.ForeColor != null ? "color:" + currentStyle.ForeColor + ";" : "")
				;
			if (result != "")
			{
				result = " style=\"" + result + "\"";
			}
			return result;
		}

		public void GetAsciiTable(StringBuilder sb)
		{
			var asciiStyles = new InnerAsciiColumnStyle[ColumnCount];
			for (int i = 0; i < ColumnCount; i++)
			{
				asciiStyles[i] = new InnerAsciiColumnStyle();
				asciiStyles[i].MaxColumnLength = GetMaxAsciiColumnLength(i, true);
				asciiStyles[i].MaxColumnLengthForRowsOnly = GetMaxAsciiColumnLength(i, false);
				Style colStyle;
				if (columnStyles.TryGetValue(i, out colStyle))
				{
					asciiStyles[i].ColumnStyle = colStyle;
				}
			}
			if (Title != null)
			{
				sb.Append(Title).AppendLine(":");
			}
			if (header != null)
			{
				AppendEveryTextElement(sb, header, asciiStyles, true);
			}
			foreach (var row in rows)
			{
				AppendEveryTextElement(sb, row, asciiStyles, false);
			}
		}

		private static void AppendEveryTextElement(StringBuilder sb, IList<CellData> values, InnerAsciiColumnStyle[] asciiStyles, bool isHeader)
		{
			for (int i = 0; i < asciiStyles.Length && i < values.Count; i++)
			{
				if (asciiStyles[i].MaxColumnLengthForRowsOnly == 0) continue; //don't display empty columns
				var colStyle = asciiStyles[i].ColumnStyle;
				var cellStyle = i < values.Count && values[i] != null ? values[i].CellStyle : null;
				var currentStyle = GetHtmlCellStyleWithDefaults(colStyle, cellStyle);

				var align = isHeader ? TextAlign.Center : currentStyle.Align.Value;
				var maxLen = asciiStyles[i].MaxColumnLength;
				var value = i < values.Count && values[i] != null && !string.IsNullOrEmpty(values[i].AsciiValue)
					? values[i].AsciiValue
					: "";

				switch (align)
				{
					case TextAlign.Left:
						sb.AppendFormat("{0,-" + maxLen + "}", value);
						break;
					case TextAlign.Right:
						sb.AppendFormat("{0," + maxLen + "}", value);
						break;
					case TextAlign.Center:
						var prefixLen = (maxLen - value.Length) / 2;
						sb.AppendFormat("{0,-" + maxLen + "}", new string(' ', prefixLen) + value);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				sb.Append(isHeader ? '/' : ' ');
			}
			sb.AppendLine();
		}

		public string GetHtmlTable()
		{
			var sb = new StringBuilder();
			GetHtmlTable(sb);
			return sb.ToString();
		}

		public string GetAsciiTable()
		{
			var sb = new StringBuilder();
			GetAsciiTable(sb);
			return sb.ToString();
		}

		private static HtmlStyle GetHtmlCellStyleWithDefaults(Style colStyle, Style cellStyle)
		{
			var result = new HtmlStyle()
			{
				Align = TextAlign.Left,
				Bold = false,
				EscapeSpace = false,
				Italic = false,
				NoWrap = false,
			};
			SetCellStyleWhereHasValue(result, colStyle);
			SetCellStyleWhereHasValue(result, cellStyle);
			return result;
		}

		private static void SetCellStyleWhereHasValue(HtmlStyle originalStyle, Style newStyle)
		{
			if (originalStyle == null || newStyle == null) return;
			if (newStyle.Align.HasValue) originalStyle.Align = newStyle.Align.Value;
			var newHtmlStyle = newStyle as HtmlStyle;
			if (newHtmlStyle == null) return;
			if (newHtmlStyle.Bold.HasValue) originalStyle.Bold = newHtmlStyle.Bold.Value;
			if (newHtmlStyle.EscapeSpace.HasValue) originalStyle.EscapeSpace = newHtmlStyle.EscapeSpace.Value;
			if (newHtmlStyle.Italic.HasValue) originalStyle.Italic = newHtmlStyle.Italic.Value;
			if (newHtmlStyle.NoWrap.HasValue) originalStyle.NoWrap = newHtmlStyle.NoWrap.Value;
			if (newHtmlStyle.ForeColor != null) originalStyle.ForeColor = newHtmlStyle.ForeColor;
		}

		private class InnerAsciiColumnStyle
		{
			public int MaxColumnLength { get; set; }
			public int MaxColumnLengthForRowsOnly { get; set; }
			public Style ColumnStyle { get; set; }
		}

		private class InnerHtmlColumnStyle
		{
			public bool IsHtmlColumnEmpty { get; set; }
			public Style ColumnStyle { get; set; }
		}

		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public enum TextAlign
		{
			Left = 0,
			Right,
			Center
		}

		public class Style
		{
			public TextAlign? Align { get; set; }
		}

		public class HtmlStyle : Style
		{
			public bool? NoWrap { get; set; }
			public bool? EscapeSpace { get; set; }
			public bool? Bold { get; set; }
			public bool? Italic { get; set; }
			public string ForeColor { get; set; }
		}

		public class CellData
		{
			public string AsciiValue { get; set; }
			public string HtmlValue { get; set; }
			public Style CellStyle { get; set; }

			public static CellData CreateFrom(string value)
			{
				return new CellData() { AsciiValue = value, HtmlValue = HttpUtility.HtmlEncode(value), };
			}

			public static CellData CreateFrom(string value, Style style)
			{
				var result = CreateFrom(value);
				result.CellStyle = style;
				return result;
			}

			private const string truncHtmlFormatString = "<SPAN title=\"{1}\">{0}</SPAN>";
			public static CellData CreateFrom(string value, int maxLength)
			{
				if (maxLength > 3 && value != null && value.Length > maxLength)
				{
					var truncValue = value.Substring(0, maxLength - 3) + "...";
					var htmlValue = string.Format(truncHtmlFormatString, HttpUtility.HtmlEncode(truncValue), HttpUtility.HtmlEncode(value));
					return new CellData() { AsciiValue = truncValue, HtmlValue = htmlValue, };
				}
				return CreateFrom(value);
			}
		}
	}

}
