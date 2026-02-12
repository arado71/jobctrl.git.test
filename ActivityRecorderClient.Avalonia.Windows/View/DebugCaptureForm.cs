using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View
{
	public partial class DebugCaptureForm : Form
	{
		private int defaultColumns;
		private readonly Dictionary<string, int> pluginColumnLookup = new Dictionary<string, int>();
		private readonly Dictionary<IntPtr, DataGridViewRow> gridRowLookup = new Dictionary<IntPtr, DataGridViewRow>();

		public DebugCaptureForm()
		{
			InitializeComponent();
			Icon = Resources.JobCtrl;
			Text = Labels.Menu_Diagnostics;
			AddDefaultColumn("Handle");
			AddDefaultColumn("IsActive");
			AddDefaultColumn("Process Id");
			AddDefaultColumn("Process Name");
			AddDefaultColumn("Title");
			AddDefaultColumn("Url");
		}

		public void SetCapture(DesktopCapture capture)
		{
			var oldColumn = gridView.SortedColumn;
			var direction = gridView.SortOrder == SortOrder.Descending
				? ListSortDirection.Descending
				: ListSortDirection.Ascending;

			foreach (var window in capture.DesktopWindows)
			{
				DataGridViewRow row;
				if (!gridRowLookup.TryGetValue(window.Handle, out row))
				{
					var rowIdx = gridView.Rows.Add(GetCells(window));
					row = gridView.Rows[rowIdx];
					gridRowLookup.Add(window.Handle, row);
				}
				else
				{
					row.SetValues(GetCells(window));
				}

				var deletedRowHandles = gridRowLookup.Keys.Except(capture.DesktopWindows.Select(x => x.Handle)).ToArray();
				foreach (var deletedRowHandle in deletedRowHandles)
				{
					var deletedRow = gridRowLookup[deletedRowHandle];
					gridView.Rows.Remove(deletedRow);
					gridRowLookup.Remove(deletedRowHandle);
				}
			}

			if (oldColumn != null)
			{
				gridView.Sort(oldColumn, direction);
			}
		}

		private object[] GetCells(DesktopWindow window)
		{
			var values = new List<object>();
			values.Add(window.Handle);
			values.Add(window.IsActive);
			values.Add(window.ProcessId);
			values.Add(window.ProcessName);
			values.Add(window.Title);
			values.Add(window.Url);
			values.AddRange(GetPluginValues(window));
			return values.ToArray();
		}

		private List<object> GetPluginValues(DesktopWindow window)
		{
			var res = new List<object>();
			if (window.CaptureExtensions != null && window.CaptureExtensions.Count > 0)
			{
				var vals = new Dictionary<int, object>();
				foreach (var plugin in window.CaptureExtensions)
				{
					vals[GetPluginValue(plugin.Key.Key, plugin.Key.Id)] = plugin.Value;
				}

				for (int i = defaultColumns; i <= vals.Keys.Max(); ++i)
				{
					object colValue;
					res.Add(vals.TryGetValue(i, out colValue) ? colValue : null);
				}
			}

			for (int i = defaultColumns + res.Count; i < gridView.ColumnCount; ++i)
			{
				res.Add(null);
			}

			return res;
		}

		private int GetPluginValue(string key, string id)
		{
			var keyid = id + " " + key;
			int result;
			if (!pluginColumnLookup.TryGetValue(keyid, out result))
			{
				result = gridView.ColumnCount;
				pluginColumnLookup.Add(keyid, result);
				AddColumn(keyid);
			}

			return result;
		}

		private void AddDefaultColumn(string header)
		{
			++defaultColumns;
			AddColumn(header);
		}

		private void AddColumn(string header)
		{
			gridView.Columns.Add("col" + header.Replace(" ", ""), header);
		}
	}
}
