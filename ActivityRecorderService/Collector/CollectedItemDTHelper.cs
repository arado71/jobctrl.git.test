using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Collector
{
	static class CollectedItemDTHelper
	{

		public static DataTable CreateDT()
		{
			DataTable dt = new DataTable("CollectedItemsDataType");
			dt.Columns.Add("UserId", typeof(Int32));
			dt.Columns.Add("CreateDate", typeof(DateTime));
			dt.Columns.Add("ComputerId", typeof(Int32));
			dt.Columns.Add("KeyId", typeof(Int32));
			dt.Columns.Add("ValueId", typeof(Int32));
			dt.Columns.Add("Key", typeof(String));
			dt.Columns.Add("Value", typeof(String));
			return dt;
		}

		public static void AddRowToDT(DataTable dt, int userId, DateTime createDate, int computerId, int? keyId, string key, int? valueId, string value)
		{
			DataRow row = dt.NewRow();

			row["UserId"] = userId;
			row["CreateDate"] = createDate;
			row["ComputerId"] = computerId;
			if (keyId != null) row["KeyId"] = keyId;
			else row["KeyId"] = DBNull.Value;
			if (valueId != null) row["ValueId"] = valueId;
			else row["ValueId"] = DBNull.Value;
			row["Key"] = (keyId == null) ? key : null;
			row["Value"] = (valueId == null) ? value : null;

			dt.Rows.Add(row);
		}

		public static void ChangeRowInDT(DataTable dt, int id, string value, bool isValueUpdate)
		{
			string columnName = isValueUpdate ? "Value" : "Key";
			var findings = (from DataRow actRow in dt.Rows
						 where HandlePossibleDbNull(actRow[columnName]) == value
						 select actRow);
			foreach (DataRow row in findings)
			{
				row[columnName + "Id"] = id;
			}
		}

		public static IEnumerable<DataRow> GetValueIdNullRows(DataTable dt)
		{
			return (from DataRow actRow in dt.Rows
					where actRow["ValueId"] == DBNull.Value
					&& actRow["Value"] != DBNull.Value
					select actRow);
		}

		public static IEnumerable<DataRow> GetKeyIdNullRows(DataTable dt)
		{
			return (from DataRow actRow in dt.Rows
					where actRow["KeyId"] == DBNull.Value
					select actRow);
		}

		private static string HandlePossibleDbNull(object value)
		{
			if (value == DBNull.Value) return null;
			return value.ToString();
		}
	}
}
