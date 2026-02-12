using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Reporter.Excel
{
	public class ExcelTableWriter<T>
	{
		private readonly string tableName;
		private readonly List<Func<T, object>> cellValueSelectors = new List<Func<T, object>>();
		private readonly List<string> columnHeaders = new List<string>();
		private readonly List<string> columnFormats = new List<string>();

		public int ColumnCount { get { return columnHeaders.Count; } }

		public ExcelTableWriter(string tableName)
		{
			this.tableName = tableName;
		}

		public void AddColumn(string columnName, Func<T, object> cellValueSelector, string format = null)
		{
			cellValueSelectors.Add(cellValueSelector);
			columnHeaders.Add(columnName);
			columnFormats.Add(format);
		}

		private static void SetRow(ExcelWorksheet sheet, int rowNumber, IEnumerable<object> values)
		{
			var i = 1;
			foreach (var value in values)
			{
				sheet.Cells[rowNumber, i++].Value = value;
			}
		}

		private IEnumerable<object> GetRow(T obj)
		{
			return cellValueSelectors.Select(cellValueSelector => cellValueSelector(obj));
		}

		public void Write(ExcelWorkbook workbook, IEnumerable<T> objects)
		{
			var sheet = workbook.Worksheets.Add(tableName);
			var currentRow = 1;
			SetRow(sheet, currentRow++, columnHeaders);
			foreach (var obj in objects)
			{
				SetRow(sheet, currentRow++, GetRow(obj));
			}

			for (var currentColumnId = 1; currentColumnId <= columnFormats.Count; ++currentColumnId)
			{
				var columnFormat = columnFormats[currentColumnId - 1];
				if (!string.IsNullOrEmpty(columnFormat))
				{
					sheet.Cells[1, currentColumnId, currentRow - 1, currentColumnId].Style.Numberformat.Format = columnFormat;
				}
			} 

			/*
			var pivotTable = sheet.PivotTables.Add(sheet.Cells[1, 1, 2, 2], tableRange, "Report");
			pivotTable.ShowHeaders = true;
			pivotTable.UseAutoFormatting = true;
			pivotTable.ApplyWidthHeightFormats = true;
			pivotTable.ShowDrill = true;
			pivotTable.FirstHeaderRow = 1;
			pivotTable.FirstDataCol = 1;
			pivotTable.FirstDataRow = 2;
			pivotTable.RowFields.Add(pivotTable.Fields["Start"]);
			//pivotTable.DataOnRows = false;*/
		}
	}
}
