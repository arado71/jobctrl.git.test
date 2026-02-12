using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Table.PivotTable;
using Reporter.Model.ProcessedItems;

namespace Reporter.Excel
{
	public static class ExcelHelper
	{
		public static void Export(string filePath, DataSet data)
		{
			if (File.Exists(filePath)) File.Delete(filePath);
			var file = new FileInfo(filePath);
			using (var excelPackage = new ExcelPackage(file))
			{
				foreach (DataTable reportTable in data.Tables)
				{
					var sheet = excelPackage.Workbook.Worksheets.Add(reportTable.TableName);
					sheet.Cells[1, 1].LoadFromDataTable(reportTable, true);
				}
				excelPackage.Save();
			}
		}

		public static void Export(string filePath, IEnumerable<WorkItem> rawData, bool writeRaw, params DataTable[] reports)
		{
			if (File.Exists(filePath)) File.Delete(filePath);
			var file = new FileInfo(filePath);
			using (var excelPackage = new ExcelPackage(file))
			{
				if (writeRaw)
				{
					var columns = rawData.SelectMany(x => x.Values.Keys).Distinct();

					var rawTable = new ExcelTableWriter<WorkItem>("Raw");
					rawTable.AddColumn("Start", x => x.StartDate, "yyyy.MM.dd HH:mm:ss.000");
					rawTable.AddColumn("End", x => x.EndDate, "yyyy.MM.dd HH:mm:ss.000");
					rawTable.AddColumn("Duration", x => x.Duration.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds, "HH:mm:ss.000");
					rawTable.AddColumn("UserId", x => x.UserId);
					rawTable.AddColumn("WorkId", x => x.WorkId);
					rawTable.AddColumn("Type", x => x.Type.ToString());
					rawTable.AddColumn("MouseAct", x => x is PcWorkItem ? ((PcWorkItem)x).MouseActivity : -1);
					rawTable.AddColumn("KeyAct", x => x is PcWorkItem ? ((PcWorkItem)x).KeyboardActivity : -1);
					rawTable.AddColumn("CompId", x => x is PcWorkItem ? ((PcWorkItem)x).ComputerId : 0);
					rawTable.AddColumn("Description", x => x is ManualWorkItem ? ((ManualWorkItem)x).Description : "");
					rawTable.AddColumn("Title", x => x is AdhocMeetingWorkItem ? ((AdhocMeetingWorkItem)x).Title : "");
					rawTable.AddColumn("Description", x => x is AdhocMeetingWorkItem ? ((AdhocMeetingWorkItem)x).Description : "");
					rawTable.AddColumn("Participants", x => x is AdhocMeetingWorkItem ? ((AdhocMeetingWorkItem)x).Participants : "");
					rawTable.AddColumn("Imei", x => x is MobileWorkItem ? ((MobileWorkItem)x).Imei : -1);

					foreach (var column in columns)
					{
						rawTable.AddColumn(column, x => x.Values != null && x.Values.ContainsKey(column) ? x.Values[column] : "");
					}

					rawTable.Write(excelPackage.Workbook, rawData);
				}

				if (reports != null)
				{
					foreach (var reportTable in reports)
					{
						var sheet = excelPackage.Workbook.Worksheets.Add(reportTable.TableName);
						sheet.Cells[1, 1].LoadFromDataTable(reportTable, true);
					}
				}

				excelPackage.Save();
			}
		}
	}
}
